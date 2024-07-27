namespace EmphatyWave.Persistence.Models
{
    public class PagedResult<T>
    {
        public ICollection<T> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
