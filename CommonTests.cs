using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common
{
    [TestClass]
    public abstract class TestBase
    {
        private DbContextOptions<DataContext> options;

        [TestInitialize]
        public void Init()
        {
            var builder = new DbContextOptionsBuilder<DataContext>();
            builder = builder.UseSqlServer(
                $"Server=(localdb)\\mssqllocaldb;Database=DynamicPropertyExpressions;Trusted_Connection=True");
            options = builder.Options;

            // Create the schema in the database
            using (var context = new DataContext(options))
            {
                context.Database.EnsureCreated();
            }
        }

        [TestMethod]
        public void BasicLinqQuery()
        {
            using (var context = new DataContext(options))
            {
                var expression = context.Set<Booking>().Where(x => x.Party.Name.Contains("foo"));
                var works = expression.ToList();
                Assert.IsFalse(works.Any());
            }
        }

        [TestMethod]
        public void ExpressionBasedQuery()
        {
            using (var context = new DataContext(options))
            {
                var bookingParameter = Expression.Parameter(typeof(Booking), "b");
                var partyParameter = Expression.Parameter(typeof(Party), "p");
                var searchTerm = Expression.Constant("foo");
                var partyPropertyOfBooking = typeof(Booking).GetProperties()[1];
                var namePropertyOfParty = typeof(Party).GetProperties()[1];

                var containsMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});
                Assert.IsNotNull(containsMethod);

                var partyNameAccessor = Expression.MakeMemberAccess(partyParameter, namePropertyOfParty);
                var containsCall = Expression.Call(partyNameAccessor, containsMethod, searchTerm);
                var subPropertyLambda = Expression.Lambda(containsCall, partyParameter);

                var bookingPartyAccessor = Expression.MakeMemberAccess(bookingParameter, partyPropertyOfBooking);

                // This is what seems to be unsupported in 3.0
                var invokeSubParameter = Expression.Invoke(subPropertyLambda, bookingPartyAccessor);

                var result = Expression.Lambda<Func<Booking, bool>>(invokeSubParameter, bookingParameter);

                var works = context.Set<Booking>().Where(result).ToList();
                Assert.IsFalse(works.Any());
            }
        }

        [TestMethod]
        public void NestedMemberAccess()
        {
            using (var context = new DataContext(options))
            {
                var bookingParameter = Expression.Parameter(typeof(Booking), "b");
                var searchTerm = Expression.Constant("foo");
                var partyPropertyOfBooking = typeof(Booking).GetProperties()[1];
                var namePropertyOfParty = typeof(Party).GetProperties()[1];

                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                Assert.IsNotNull(containsMethod);

                var bookingPartyAccessor = Expression.MakeMemberAccess(bookingParameter, partyPropertyOfBooking);
                // this is the way that it seems to get structured in ef core 3. e.g. (b.Party).Name 
                var partyNameAccessor = Expression.MakeMemberAccess(bookingPartyAccessor, namePropertyOfParty);
                var containsCall = Expression.Call(partyNameAccessor, containsMethod, searchTerm);

                var result = Expression.Lambda<Func<Booking, bool>>(containsCall, bookingParameter);

                var works = context.Set<Booking>().Where(result).ToList();
                Assert.IsFalse(works.Any());
            }
        }
    }
}
