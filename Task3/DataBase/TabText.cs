namespace DataBase
{
    public class TabText
    {
        public int Id { get; set; }
        public string Text { get; set; }
        virtual public ICollection<QuestionAnswer> Answers { get; set; }
    }
}