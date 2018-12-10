using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalGo.DataExchanger.Compilers;
using System.Collections.Generic;
using System.Linq;

namespace SignalGoTest.DataExhanger
{
    [TestClass]
    public class QueryDataExchangerAllTests
    {
        [TestMethod]
        public void Example1()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali""}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();

            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali").ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example2()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" and user.family = ""yousefi""}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();

            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali" && x.Family == "yousefi").ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example3()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" and user.family = ""yousefi"" or (user.name == ""reza"" or user.family == ""jamal"")}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();

            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali" && x.Family == "yousefi" || (x.Name == "reza" || x.Family == "jamal")).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example4()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" and user.family = ""yousefi"" or (user.name == ""reza"" or user.family == ""jamal"" or (user.family == ""jamal""))}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();

            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali" && x.Family == "yousefi" || (x.Name == "reza" && x.Family == "jamal" || (x.Family == "jamal"))).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example5()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" or user.name = ""ali"" and (user.family == ""jamal"")}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();

            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali" || x.Name == "ali" && (x.Family == "jamal")).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example6()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" or user.name = ""ali"" and (user.family == ""jamal"") or user.family == ""jamal""}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali" || x.Name == "ali" && (x.Family == "jamal") || x.Family == "jamal").ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example7()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" or count(user.posts) = 1}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" || x.Posts.Count() == 1).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example8()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" or count ( user.posts ) = 1 or count(user.posts)=0}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" || x.Posts.Count() == 1 || x.Posts.Count() == 0).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example9()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" or count ( user.posts ) != count(user.posts)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" || x.Posts.Count() != x.Posts.Count()).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example10()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" or(user.name == ""ali"" and count ( user.posts ) >0)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" || (x.Name == "ali" && x.Posts.Count() > 0)).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);

        }

        [TestMethod]
        public void Example11()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" and sum ( 5 , 1 , 4 ) == 10)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" && 5 + 1 + 4 == 10).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);

        }

        [TestMethod]
        public void Example12()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" and sum(5,1,4)==10)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" && 5 + 1 + 4 == 10).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);

        }

        [TestMethod]
        public void Example13()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where sum(count(user.posts),count(user.posts),count(user.posts))==6)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => (x.Posts.Count() + x.Posts.Count() + x.Posts.Count()) == 6).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example14()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where sum(sum(count(user.posts),count(user.posts)),count(user.posts))==6)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => ((x.Posts.Count() + x.Posts.Count()) + x.Posts.Count()) == 6).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Example15()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}
                                    var user
                                    {
                                        where sum(sum(count(user.posts),count(user.posts)),count(user.posts))==6)
                                        var post in user.posts
	                                    {
		                                    where post.Title = ""ali""
	                                    }
                                    }";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => ((x.Posts.Count() + x.Posts.Count()) + x.Posts.Count()) == 6).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.IsTrue(equal);
        }
    }
}
