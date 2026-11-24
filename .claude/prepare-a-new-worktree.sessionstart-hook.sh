#!/bin/sh
# SessionStart hook: prepares a linked worktree of JAStudio's source repository for the Definition of Done (full-build.ps1, the .NET
# specifications, pytest) when a session starts in it. git gives a new worktree the tracked files only, and these need two things more:
#
# - venv: linked to the main checkout's venv, a directory junction on Windows and a symbolic link elsewhere. A venv of its own would be a
#   large install (Anki, Qt, pythonnet), and the Definition of Done only reads it. A pip install in the worktree changes the main
#   checkout's venv.
# - submodules/pythonnet-stub-generator: checked out, since full-build.ps1 regenerates the Python type stubs with it.
#
# Not the corpus, src/jas_database: nothing in the Definition of Done reads it, and checking it out takes over a minute. The work that
# does, the parsing cases' full mode and the headless JAStudio, says to run dev_scripts/check-out-the-corpus.ps1 when it's missing.
#
# It does nothing in the main checkout, nor for what the worktree already has, so it is silent in every session but a worktree's first.
# What it did, and what failed, it says on stdout with exit 0: the only way a SessionStart hook reaches the agent.
checkout=${CLAUDE_PROJECT_DIR:-$(git -C "$(dirname -- "$0")" rev-parse --show-toplevel)}
git_folder=$(git -C "$checkout" rev-parse --absolute-git-dir) || exit 0
common_git_folder=$(git -C "$checkout" rev-parse --path-format=absolute --git-common-dir) || exit 0
[ "$git_folder" = "$common_git_folder" ] && exit 0 # The main checkout.
main_checkout=$(dirname -- "$common_git_folder")

done_here=""
failed_here=""

link_the_venv() {
   venv="$checkout/venv"
   if [ -e "$venv" ] || [ -L "$venv" ]; then return; fi
   main_venv="$main_checkout/venv"
   if [ ! -f "$main_venv/pyvenv.cfg" ]; then
      failed_here="$failed_here This worktree has no venv, and the main checkout has none to link it to at $main_venv: run setup-dev.ps1 there, then start a new session here."
      return
   fi
   case "$(uname -s)" in
      # Git Bash would turn the switches into paths, /J into J:\, without MSYS2_ARG_CONV_EXCL.
      MINGW* | MSYS* | CYGWIN*) MSYS2_ARG_CONV_EXCL='*' cmd /c mklink /J "$(cygpath -w "$venv")" "$(cygpath -w "$main_venv")" >/dev/null 2>&1 ;;
      *) ln -s "$main_venv" "$venv" ;;
   esac
   if [ -f "$venv/pyvenv.cfg" ]; then
      done_here="$done_here Linked venv to the main checkout's venv, $main_venv: it's shared, so don't pip install here."
   else
      failed_here="$failed_here Linking venv to the main checkout's venv, $main_venv, failed."
   fi
}

check_out_the_stub_generator() {
   stub_generator=submodules/pythonnet-stub-generator
   if [ -e "$checkout/$stub_generator/.git" ]; then return; fi
   if why=$(git -C "$checkout" submodule update --init --quiet -- "$stub_generator" 2>&1); then
      done_here="$done_here Checked out the submodule $stub_generator, which full-build.ps1 regenerates the Python type stubs with."
   else
      failed_here="$failed_here Checking out the submodule $stub_generator failed, so full-build.ps1 will: $why"
   fi
}

link_the_venv
check_out_the_stub_generator

if [ -n "$done_here" ] || [ -n "$failed_here" ]; then
   echo "Preparing this new worktree of JAStudio for the Definition of Done:$done_here$failed_here"
   if [ ! -e "$checkout/src/jas_database/.git" ]; then
      echo "The corpus, src/jas_database, isn't checked out: the Definition of Done doesn't need it. For work that reads it, such as the parsing cases' full mode or the headless JAStudio, run dev_scripts/check-out-the-corpus.ps1 first."
   fi
fi
exit 0
