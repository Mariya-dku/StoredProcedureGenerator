// -----------------------------------------------------------------------
// <copyright file="StoredProcedureGenerationTests.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace StoredProcedureSourceGenerator.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StoredProcedureGenerationTests
    {
        [TestMethod]
        public void MapResultSetToProcedure()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial IList<Item> M()
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial IList<Item> M()
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            var result = this.dbContext.Items.FromSqlRaw(sqlQuery).ToList();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void MapSingleObjectToProcedure()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial Item M()
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial Item M()
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            var result = this.dbContext.Items.FromSqlRaw(sqlQuery).AsEnumerable().FirstOrDefault();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ParametersPassedToStoredProcedure()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial IList<Item> M(string clientId, string personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial IList<Item> M(string clientId, string personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId == null ? (object)DBNull.Value : clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.Value = personId == null ? (object)DBNull.Value : personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id"";
            var result = this.dbContext.Items.FromSqlRaw(sqlQuery, parameters).ToList();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ContextNameFoundFromClass()
        {
            string source = @"
namespace Foo
{
    public partial class CustomDbContext : DbContext
    {
    }

    class C
    {
        private readonly CustomDbContext context;

        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        internal partial IList<Item> M(string clientId, string personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        internal partial IList<Item> M(string clientId, string personId)
        {
            var connection = this.context.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId == null ? (object)DBNull.Value : clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.Value = personId == null ? (object)DBNull.Value : personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id"";
            var result = this.context.Items.FromSqlRaw(sqlQuery, parameters).ToList();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void DbSetNameFoundFromClass()
        {
            string source = @"
namespace Foo
{
    public partial class CustomDbContext : DbContext
    {
        public virtual DbSet<PersonItem>? PersonItems { get; set; } = null!;
    }

    class C
    {
        private readonly CustomDbContext context;

        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial IList<PersonItem> M(string clientId, string personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial IList<PersonItem> M(string clientId, string personId)
        {
            var connection = this.context.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId == null ? (object)DBNull.Value : clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.Value = personId == null ? (object)DBNull.Value : personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id"";
            var result = this.context.PersonItems.FromSqlRaw(sqlQuery, parameters).ToList();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void DbSetNameFoundFromClass2()
        {
            string source = @"
namespace Foo
{
    public partial class CustomDbContext : DbContext
    {
        public virtual DbSet<Item>? Items { get; set; } = null!;
        public virtual DbSet<PersonItem>? Persons { get; set; } = null!;
    }

    class C
    {
        private readonly CustomDbContext context;

        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial IList<PersonItem> M(string clientId, string personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial IList<PersonItem> M(string clientId, string personId)
        {
            var connection = this.context.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId == null ? (object)DBNull.Value : clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.Value = personId == null ? (object)DBNull.Value : personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id"";
            var result = this.context.Persons.FromSqlRaw(sqlQuery, parameters).ToList();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void NonReferenceParameterPassedDirectlyToStoredProcedure()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial IList<Item> M(int clientId, int? personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial IList<Item> M(int clientId, int? personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.Value = personId == null ? (object)DBNull.Value : personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id"";
            var result = this.dbContext.Items.FromSqlRaw(sqlQuery, parameters).ToList();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void NonReferenceParameterPassedDirectlyToStoredProcedureForNullable()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial IList<Item> M(int clientId, int? personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Enable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial IList<Item> M(int clientId, int? personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.Value = personId == null ? (object)DBNull.Value : personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id"";
            var result = this.dbContext.Items.FromSqlRaw(sqlQuery, parameters).ToList();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void OutputParameters()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial IList<Item> M(int clientId, out int? personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial IList<Item> M(int clientId, out int? personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.Int32;
            personIdParameter.Direction = System.Data.ParameterDirection.Output;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            var result = this.dbContext.Items.FromSqlRaw(sqlQuery, parameters).ToList();
            personId = personIdParameter.Value == DbNull.Value ? (int?)null : (int?)personIdParameter.Value;
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void StringOutputParameters()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial IList<Item> M(int clientId, out string personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial IList<Item> M(int clientId, out string personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.String;
            personIdParameter.Direction = System.Data.ParameterDirection.Output;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            var result = this.dbContext.Items.FromSqlRaw(sqlQuery, parameters).ToList();
            personId = personIdParameter.Value == DbNull.Value ? (string)null : (string)personIdParameter.Value;
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void IntOutputParameters()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial IList<Item> M(int clientId, out int personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial IList<Item> M(int clientId, out int personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.Int32;
            personIdParameter.Direction = System.Data.ParameterDirection.Output;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            var result = this.dbContext.Items.FromSqlRaw(sqlQuery, parameters).ToList();
            personId = (int)personIdParameter.Value;
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void IntRefParameters()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial IList<Item> M(int clientId, ref int personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial IList<Item> M(int clientId, ref int personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.Int32;
            personIdParameter.Direction = System.Data.ParameterDirection.InputOutput;
            personIdParameter.Value = personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            var result = this.dbContext.Items.FromSqlRaw(sqlQuery, parameters).ToList();
            personId = (int)personIdParameter.Value;
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ScalarResult()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial int M(int clientId, string? personId);
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial int M(int clientId, string? personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.Value = personId == null ? (object)DBNull.Value : personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id"";
            command.CommandText = sqlQuery;
            command.Parameters.AddRange(parameters);
            this.dbContext.Database.OpenConnection();
            try
            {
                var result = command.ExecuteScalar();
                return (int)result;
            }
            finally
            {
                this.dbContext.Database.CloseConnection();
            }
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ScalarResultWithNullableStringOutput()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial int M(int clientId, out string? personId);
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial int M(int clientId, out string? personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.String;
            personIdParameter.Direction = System.Data.ParameterDirection.Output;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            command.CommandText = sqlQuery;
            command.Parameters.AddRange(parameters);
            this.dbContext.Database.OpenConnection();
            try
            {
                var result = command.ExecuteScalar();
                personId = personIdParameter.Value == DbNull.Value ? (string?)null : (string?)personIdParameter.Value;
                return (int)result;
            }
            finally
            {
                this.dbContext.Database.CloseConnection();
            }
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ScalarResultWithStringOutput()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial int M(int clientId, out string personId);
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial int M(int clientId, out string personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.String;
            personIdParameter.Direction = System.Data.ParameterDirection.Output;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            command.CommandText = sqlQuery;
            command.Parameters.AddRange(parameters);
            this.dbContext.Database.OpenConnection();
            try
            {
                var result = command.ExecuteScalar();
                personId = personIdParameter.Value == DbNull.Value ? (string)null : (string)personIdParameter.Value;
                return (int)result;
            }
            finally
            {
                this.dbContext.Database.CloseConnection();
            }
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ScalarResultWithIntOutput()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [StoredProcedureGeneratedAttribute(""sp_TestSP"")]
        public partial int M(int clientId, out int personId);
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial int M(int clientId, out int personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.Int32;
            personIdParameter.Direction = System.Data.ParameterDirection.Output;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            command.CommandText = sqlQuery;
            command.Parameters.AddRange(parameters);
            this.dbContext.Database.OpenConnection();
            try
            {
                var result = command.ExecuteScalar();
                personId = (int)personIdParameter.Value;
                return (int)result;
            }
            finally
            {
                this.dbContext.Database.CloseConnection();
            }
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        private string GetGeneratedOutput(string source, NullableContextOptions nullableContextOptions)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            var compilation = CSharpCompilation.Create(
                "foo",
                new SyntaxTree[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContextOptions));

            // var compileDiagnostics = compilation.GetDiagnostics();
            // Assert.IsFalse(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());
            ISourceGenerator generator = new Generator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
            Assert.IsFalse(generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

            string output = outputCompilation.SyntaxTrees.Last().ToString();

            Console.WriteLine(output);

            return output;
        }
    }
}
