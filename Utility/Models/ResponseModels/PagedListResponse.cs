using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Utility.Models.ResponseModels
{
    public class PagedListResponse<T> : List<T>
    {
        public PagedListResponse(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            TotalCount = count;
            AddRange(items);
        }
        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public static async Task<PagedListResponse<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedListResponse<T>(items, count, pageNumber, pageSize);
        }
        public static async Task<PagedListResponse<T>> CreateAsync(List<T> source, int pageNumber, int pageSize)
        {
            var count = await Task.Run(() => source.Count);
            var items = await Task.Run(() => source.AsEnumerable().Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
            return new PagedListResponse<T>(items, count, pageNumber, pageSize);
        }
    }
}
