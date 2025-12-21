
using Base.DAL.Models.BaseModels;
using Base.Repo.Specifications;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Base.Repo.Implementations
{
    public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
    {
        // دالة وقائية ثابتة (Static) تطبق المواصفات على الاستعلام
        public static IQueryable<TEntity> GetQuery(
            IQueryable<TEntity> inputQuery,
            ISpecification<TEntity> specification)
        {
            var query = inputQuery;

            // 1. 🟢 التصفية (Criteria) - وقائي: يتم تطبيق Where أولاً
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // 2. 🟢 التحميل المُضمن البسيط (Includes)
            // هذا السطر يكفي للتعامل مع الـ Includes العادية بكفاءة
            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

            // 3. 🟢 التحميل المتداخل (Nested Includes / ThenInclude) - هام جداً للأداء
            // هذا الجزء يعالج قائمة 'AllIncludes' الموجودة في BaseSpecification
            if (specification.AllIncludes != null)
            {
                foreach (var include in specification.AllIncludes)
                {
                    // نقوم بتنفيذ دالة الـ Include وتحديث الـ query بالنتيجة
                    query = include(query);
                }
            }

            // 4. 🟢 الترتيب (Ordering)
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // 5. 🟢 التصفح (Paging) - وقائي: نتحقق من تفعيل التصفح
            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip)
                             .Take(specification.Take);
            }

            return query;
        }
    }
}

















//using Base.DAL.Models.BaseModels;
//using Base.Repo.Specifications;
//using Microsoft.EntityFrameworkCore;

//namespace Base.Repo.Implementations
//{
//    public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
//    {
//        // دالة وقائية ثابتة (Static) تطبق المواصفات على الاستعلام
//        public static IQueryable<TEntity> GetQuery(
//            IQueryable<TEntity> inputQuery,
//            ISpecification<TEntity> specification)
//        {
//            var query = inputQuery;

//            // 1. 🟢 التصفية (Criteria) - وقائي: يتم تطبيق Where أولاً
//            if (specification.Criteria != null)
//            {
//                query = query.Where(specification.Criteria);
//            }

//            // 2. 🟢 التحميل المُضمن (Includes) - وقائي: يتم تجميع العلاقات
//            // ApplySpecification في GenericRepository لدينا قام بتطبيق AsNoTracking() بالفعل
//            // لكننا نضمن أن الـ Includes يتم تطبيقها هنا.
//            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

//            // 3. 🟢 الترتيب (Ordering)
//            if (specification.OrderBy != null)
//            {
//                query = query.OrderBy(specification.OrderBy);
//            }
//            else if (specification.OrderByDescending != null)
//            {
//                query = query.OrderByDescending(specification.OrderByDescending);
//            }

//            if (specification.Includes.Any())
//            {
//                foreach (var specificationInclude in specification.Includes)
//                {
//                    query.Include(specificationInclude);
//                }
//            }

//            // 4. 🟢 التصفح (Paging) - وقائي: نتحقق من تفعيل التصفح
//            if (specification.IsPagingEnabled)
//            {
//                query = query.Skip(specification.Skip)
//                             .Take(specification.Take);
//            }

//            return query;
//        }
//    }
//}
