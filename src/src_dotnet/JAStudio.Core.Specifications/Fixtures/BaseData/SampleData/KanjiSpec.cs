using System;
using System.Collections.Generic;

namespace JAStudio.Core.Specifications.Fixtures.BaseData.SampleData;

public class KanjiSpec : IEquatable<KanjiSpec>
{
   public KanjiSpec(string question, string answer, string onReadings, string kunReadings)
   {
      Question = question;
      Answer = answer;
      OnReadings = onReadings;
      KunReadings = kunReadings;
   }

   public string Question { get; }
   public string Answer { get; }
   public string OnReadings { get; }
   public string KunReadings { get; }

   public override string ToString() => $"""KanjiSpec("{Question}", "{Answer}", "{OnReadings}", "{KunReadings}")""";

   public override bool Equals(object? obj) => Equals(obj as KanjiSpec);

   public bool Equals(KanjiSpec? other) =>
      other is not null
   && other.Question == Question
   && other.Answer == Answer
   && other.KunReadings == KunReadings
   && other.OnReadings == OnReadings;

   public override int GetHashCode() => Question.GetHashCode();

   public static readonly List<KanjiSpec> TestKanjiList =
   [
      new("病", "illness", "<primary>びょう</primary>", "<primary>や</primary>, <primary>やまい</primary>"),
      new("品", "goods | elegance", "<primary>ひん</primary>", "<primary>しな</primary>"),
      new("塚", "a-{mound/aoeu}", "ちょう", "<primary>つか</primary>"),
      new("疒", "sick", "", ""),
      new("丙", "dynamite", "", ""),
      new("一", "one/(ground)", "いち, いつ", "ひと"),
      new("冂", "(head)", "", ""),
      new("人", "person", "にん, じん", "ひと, と"),
      new("女", "female", "じょ, にょ, にょう", "おんな, め"),
      new("本", "true/main | book", "ほん", "もと"),
      new("夕", "evening", "せき", "ゆう"),
      new("出", "exit|take-out", "しゅつ", "で, だ"),
      new("中", "inside/center", "ちゅう", "なか"),
      new("日", "day | sun", "にち, じつ", "ひ, か, び"),
      new("水", "water", "すい", "みず"),
      new("友", "friend", "ゆう", "とも"),
      new("内", "inside/within", "ない", "<primary>うち</primary>"),
      new("生", "life", "せい, しょう", "い, なま, う, は, き"),
      new("今", "now", "こん", "いま"),
      new("分", "part/portion|understand/know", "ぶん, ふん, ぶ", "わ"),
      new("気", "spirit/mind | atmosphere/mood", "き, け", ""),
      new("先", "previous/last | tip/edge/ahead", "せん", "さき, ま"),
      new("見", "See", "けん", "み"),
      new("町", "town/block/street", "ちょう", "まち"),
      new("角", "corner", "かく", "かど, つの"),
      new("体", "body|object|substance", "たい", "からだ"),
      new("色", "color", "しき, しょく", "いろ"),
      new("来", "come(ing)", "らい", "く, き"),
      new("当", "hit | correct/appropriate", "とう", "あ"),
      new("行", "going|carry-out", "こう, ぎょう", "い, おこな, ゆ"),
      new("会", "Meet, Meeting", "かい", "あ"),
      new("米", "rice/america", "べい, まい", "こめ"),
      new("言", "say | word", "げん, ごん", "い, こと"),
      new("自", "self(-)", "じ, し", "みずか"),
      new("別", "separate/fork", "べつ", "わか"),
      new("特", "Special", "とく", ""),
      new("合", "suit/fit | join", "ごう, がっ", "あ, あい"),
      new("様", "Formal Name Title, Formal Name Ender, Manner", "よう", "さま"),
      new("然", "sort-of-thing/nature", "ぜん, ねん", "しか, さ"),
      new("問", "question | problem", "もん", "と, とん"),
      new("題", "Topic", "だい", ""),
      new("折", "Fold, Bend, Break", "せつ", "お, おり"),
      new("性", "gender | nature | sex", "せい, しょう", ""),
      new("松", "Pine, Pine Tree", "しょう", "まつ"),
      new("秋", "Autumn, Fall", "しゅう", "あき"),
      new("晴", "clear-up", "せい", "は"),
      new("泣", "cry", "きゅう", "な"),
      new("丈", "height | sturdy/stout", "じょう", "たけ, だけ"),
      new("計", "plan/calculate | measure", "けい", "はか"),
      new("昼", "noon", "ちゅう", "ひる"),
      new("昨", "previous/yesterday", "さく", ""),
      new("存", "Exist, Suppose", "そん, ぞん", ""),
      new("守", "protect", "す, しゅ", "まも, もり"),
      new("取", "take", "しゅ", "と"),
      new("書", "write|writing", "しょ", "か"),
      new("是", "Absolutely", "ぜ", ""),
      new("識", "Discerning, Discriminating, Know", "しき", ""),
      new("敗", "failure/defeat", "はい", "やぶ"),
      new("無", "un-/non- | nothing", "む, ぶ", "な"),
      new("達", "reach|attain|{k:pluralizing-suffix}", "たつ", "たち"),
      new("報", "information/report | {k:reward|retaliate}", "ほう", "むく"),
      new("善", "Morally Good, Good", "ぜん", ""),
      new("夢", "dream", "む", "ゆめ"),
      new("在", "Exist", "ざい", ""),
      new("余", "surplus/excess", "よ", "あま"),
      new("素", "Element", "そ, す", "もと"),
      new("寝", "lie-down/sleep", "しん", "ね"),
      new("応", "respond | accept", "おう", "こた"),
      new("宮", "(shinto)shrine | palace", "きゅう", "みや"),
      new("観", "View", "かん", "み"),
      new("藤", "Wisteria", "とう, どう", "ふじ"),
      new("護", "Defend", "ご", ""),
      new("視", "Look At", "し", "み"),
      new("居", "reside/exist(animate)", "きょ", "い"),
      new("掛", "put-on/apply/consume", "かい, けい", "か"),
      new("捜", "search", "そう", "さが"),
      new("往", "Journey, Depart", "おう", ""),
      new("誘", "Invite", "ゆう", "さそ"),
      new("処", "Deal With", "しょ", "ところ"),
      new("恐", "fear", "きょう", "おそ, こわ"),
      new("怖", "scary", "ふ", "こわ"),
      new("遊", "play", "ゆう", "あそ"),
      new("離", "separation", "り", "はな"),
      new("幾", "How Many, How Much", "き", "いく"),
      new("探", "search-for", "たん", "さが, さぐ"),
      new("祖", "Ancestor", "そ", ""),
      new("捨", "throw-away", "しゃ", "す"),
      new("欲", "want", "よく", "ほ"),
      new("傷", "wound/injure", "しょう", "きず, いた"),
      new("刻", "Carve", "こく", "きざ"),
      new("奴", "{や:guy} | {ど:slave}", "ど", "やつ"),
      new("考", "Think, Consider", "こう", "かんが"),
      new("何", "what", "か", "なに, なん"),
      new("全", "all/entire", "ぜん", "すべ, まった"),
      new("明", "bright", "めい, みょう", "あ, あか, あき"),
      new("食", "eat | food", "しょく", "た, く"),
      new("前", "in-front-of(space)|before(time)", "ぜん", "まえ"),
      new("有", "possess | exists | happen/occur", "ゆう, う", "あ"),
      new("私", "private | I", "し", "わたし"),
      new("知", "Know", "ち", "し"),
      new("夜", "Night, Evening", "や", "よ, よる"),
      new("海", "sea", "かい", "うみ"),
      new("羊", "sheep", "よう", "ひつじ"),
      new("付", "attach", "ふ", "つ"),
      new("失", "loss | error/fault", "しつ", "うしな"),
      new("時", "time/hour", "じ", "とき"),
      new("記", "write | account", "き", "しる"),
      new("答", "answer/response | solution", "とう", "こた"),
      new("楽", "comfort/ease", "らく, がく", "たの"),
      new("助", "help/assist", "じょ", "たす, すけ"),
      new("所", "place", "しょ", "ところ"),
      new("身", "some(body)", "しん", "み"),
      new("話", "tale/talk", "わ", "はな, はなし"),
      new("事", "incorporeal-thing(matter|event|action|situation)", "じ", "こと, つか"),
      new("度", "degree | time(s)", "ど, たく", "たび"),
      new("要", "Need", "よう", "い, かなめ"),
      new("部", "department | club", "ぶ", "へ"),
      new("終", "end/finish", "しゅう", "おわ, お"),
      new("落", "Fall", "らく", "お"),
      new("頭", "Head", "ず, とう", "あたま, かしら"),
      new("聞", "hear | ask | listen", "ぶん, もん", "き"),
      new("調", "Investigate, Tone", "ちょう", "しら"),
      new("親", "parent|intimacy", "しん", "おや, した"),
      new("成", "(grow-to)become", "せい", "な"),
      new("命", "fate | life", "めい, みょう", "いのち"),
      new("良", "good/pleasing", "りょう", "よ, い"),
      new("好", "like", "こう", "す, この"),
      new("吉", "Good Luck", "きつ, きち", "よし"),
      new("俺", "I, Me", "", "おれ"),
      new("如", "Likeness", "じょ", "ごと"),
      new("遂", "accomplish", "すい", "と, つい"),
      new("勘", "Intuition", "かん", ""),
      new("佳", "Excellent, Skilled", "か", ""),
      new("癒", "Healing, Cure", "ゆ", "い, いや"),
      new("梓", "Japanese Birch, Birch", "し", "あずさ"),
      new("逝", "Die", "せい", "い, ゆ"),
      new("彼", "he | they/that/the", "ひ", "かれ, かの"),
      new("己", "Oneself, Self", "こ, き", "おのれ"),
      new("為", "do|benefit", "い", "ため, な, す"),
      new("戻", "Return", "れい", "もど"),
      new("宜", "best-regards|good", "ぎ", "よろ"),
      new("逃", "Escape", "とう", "に, のが, の"),
      new("懸", "Suspend", "けん", "か"),
      new("緒", "Together", "しょ, ちょ", "お"),
      new("免", "Excuse", "めん", "まぬか"),
      new("御", "honorable", "ご, ぎょ", "お, おん, み"),
      new("貰", "get/obtain", "せい, しゃ", "もら.う"),
      new("逢", "meetingtrystdate", "ほう", "あ.う, むか.える")
   ];
}
