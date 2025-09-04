using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Web.Tests
{
    [CollectionDefinition("web")]
    public class MyWebAppCollection : ICollectionFixture<MyWebApplicationFactory>
    {
        // 此类不需要包含任何代码，其目的仅用于定义集合
    }
}
