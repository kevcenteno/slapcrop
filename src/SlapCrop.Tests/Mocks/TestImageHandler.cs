using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SlapCrop.Tests.Mocks
{
    class TestImageHandler : ImageHandler
    {
        protected override ImageProcessRunner GetProcessRunner(HttpContextBase context)
        {
            return new TestImageRunner(context);
        }
    }
}
