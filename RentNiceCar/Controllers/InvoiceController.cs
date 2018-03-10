using Microsoft.AspNet.Identity;
using RentNiceCar.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace RentNiceCar.Controllers
{
    public class InvoiceController : Controller
    {
        internal static InvoiceController Instance;
        private ApplicationDbContext db = new ApplicationDbContext();

        public static Car GetCarById(string Kenteken)
        {
            return Instance.db.Cars.Find(Kenteken);
        }

        public static ApplicationUser GetUserById(string Kenteken)
        {
            return Instance.db.Users.Find(Kenteken);
        }
        
        // GET: Invoices
        [Authorize]
        public ActionResult Index()
        {
            Instance = this;
            string userId = User.Identity.GetUserId();
            return View(db.Invoices.Where(x => x.UserId == userId).ToList());
        }

        // GET: Invoice
        [Authorize]
        public ActionResult View(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();
            Instance = this;

            var Invoice = db.Invoices.Find(id.Value);
            if (Invoice == null)
                return HttpNotFound();

            if(User.Identity.GetUserId() != Invoice.UserId && !User.IsInRole("Medewerker") && !User.IsInRole("Eigenaar"))
                return HttpNotFound();

            return View(Invoice);
        }

        // GET: Manage Invoices
        [Authorize( Roles = "Eigenaar, Medewerker")]
        public ActionResult Manage()
        {
            Instance = this;
            return View(db.Invoices.Where(x => !x.InleverDatum.HasValue).OrderBy(x => x.VanafDatum).ToList());
        }

        // GET: Edit Invoices
        [Authorize( Roles = "Eigenaar, Medewerker")]
        public ActionResult Edit(int? id)
        {
            Instance = this;

            if(!id.HasValue)
                return HttpNotFound();

            var Invoice = db.Invoices.Find(id.Value);
            if (Invoice == null)
                return HttpNotFound();
            
            return View(Invoice);
        }
        
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize (Roles = "Eigenaar, Medewerker")]
        public ActionResult Edit([Bind(Include = "InvoiceId,Kenteken,UserId,VanafDatum,TotDatum,InleverDatum")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(invoice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Manage");
            }
            return View(invoice);
        }

        // GET: Car/Delete/5
        [Authorize(Roles = "Eigenaar, Medewerker")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // POST: Car/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Eigenaar, Medewerker")]
        public ActionResult DeleteConfirmed(int id)
        {
            var invoice = db.Invoices.Find(id);
            db.Invoices.Remove(invoice);
            db.SaveChanges();
            return RedirectToAction("Manage");
        }
    }
}