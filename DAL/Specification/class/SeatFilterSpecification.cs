using BLL.Dto;
using DAL.Data;
using DAL.Specification.Class;
using System;

namespace DAL.Specification
{
    public class SeatFilterSpecification : Specification<Seat>
    {
        public SeatFilterSpecification(SeatDto dto)
            : base(s =>
                // 1. لو الـ EventId مبعوتش، يتجاهل الشرط. لو مبعوت، يقارن بيه.
                (!dto.EventId.HasValue || dto.EventId == Guid.Empty || s.EventId == dto.EventId.Value) &&

                // 2. فلترة النصوص (لو فاضية يتجاهلها)
                (string.IsNullOrEmpty(dto.Section) || s.Section == dto.Section) &&
                (string.IsNullOrEmpty(dto.Row) || s.Row == dto.Row) &&
                (string.IsNullOrEmpty(dto.Number) || s.Number == dto.Number) &&

                // 3. لو الـ Status مبعوتش، يتجاهل الشرط. لو مبعوت، يفلتر بيه.
                (!dto.Status.HasValue || s.Status == dto.Status.Value)
            )
        {
        }
    }
}