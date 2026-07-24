using DAL.Data;
using DAL.Specification.Class;


    public class EventWithFiltersSpecification : Specification<Event>
    {
        public EventWithFiltersSpecification(DAL.Specification.EventQueryParameters queryParams)
            : base(e =>
                (string.IsNullOrEmpty(queryParams.Search) || e.Title.Contains(queryParams.Search) || e.Description.Contains(queryParams.Search)) &&
                (string.IsNullOrEmpty(queryParams.Place) || e.Place.Contains(queryParams.Place))&&
                (string.IsNullOrEmpty(queryParams.Name) || e.Title.Contains(queryParams.Name))
            )
        {
        }

     
        
    }
