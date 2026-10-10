using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Utility
{
    public class RazorHelper
    {
        /// <summary>Renders a view to a string. Awaits the rendering, so the full HTML is returned.</summary>
        public static async Task<string> RenderRazorViewToStringAsync(Controller controller, string viewName, object? model = null)
        {
            controller.ViewData.Model = model;
            await using var sw = new StringWriter();
            if (controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) is not ICompositeViewEngine viewEngine)
            {
                return string.Empty;
            }
            ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);
            if (viewResult.View == null)
            {
                return string.Empty;
            }
            ViewContext viewContext = new(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw, new HtmlHelperOptions());
            await viewResult.View.RenderAsync(viewContext);
            return sw.GetStringBuilder().ToString();
        }
    }
}
