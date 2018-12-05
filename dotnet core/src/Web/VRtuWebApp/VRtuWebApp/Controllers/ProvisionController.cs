using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VRtuWebApp.Models;

namespace VRtuWebApp.Controllers
{
    public class ProvisionController : Controller
    {
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcefghijklmnopqrtstuvwxyz0123456789";
        // GET: Provision
        public ActionResult Index()
        {
            return View();
        }

        // GET: Provision/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Provision/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(int unitId, string virtualRtuId, string deviceId, string moduleId, int expirationMinutes)
        {
            try
            {
                DateTime created = DateTime.UtcNow;
                DateTime expiration = created.AddMinutes(expirationMinutes);

                string luss = GetLuss();

                //update the table entity and return the luss

                LussEntity entity = new LussEntity()
                {
                    Luss = luss,
                    DeviceId = deviceId,
                    ModuleId = moduleId,
                    UnitId = unitId,
                    VirtualRtuId = virtualRtuId,
                    Created = created,
                    Expires = expiration
                };

                Task task = entity.UpdateAsync("DefaultEndpointsProtocol=https;AccountName=virtualrtu;AccountKey=m9oVIL1QaGEXADxHxWRCdfYLjUJeTOhgP+Qlx8VWu4DQmTZvvfxRWoE/gKilFotHNkhiF574a/V1jPu/DdrXcQ==;EndpointSuffix=core.windows.net");
                Task.WhenAll(task);

                ViewBag.LUSS = luss;
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.InnerException.Message);
            }

            return View();
        }


        private string GetLuss()
        {
            int len = alphabet.Length - 1;
            Random ran = new Random();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                int id = ran.Next(0, len);
                builder.Append(alphabet[id]);
            }

            return builder.ToString();
        }

        // POST: Provision/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Provision/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Provision/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Provision/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Provision/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}