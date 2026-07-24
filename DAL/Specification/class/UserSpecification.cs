using DAL.Data.AuthModel;
using DAL.Specification.Class;
using System;

namespace DAL.Specification
{
    public class UserSpecification : Specification<App_user>
    {
        public UserSpecification(UserQueryParameters queryParams)
            : base(u => string.IsNullOrEmpty(queryParams.Search) ||
                       u.f_name.ToLower().Contains(queryParams.Search) ||
                       u.l_name.ToLower().Contains(queryParams.Search) ||
                       u.Email!.ToLower().Contains(queryParams.Search))
        { 
            ApplyPagination(queryParams.PageSize * (queryParams.PageIndex - 1), queryParams.PageSize);
        } 
    }
}