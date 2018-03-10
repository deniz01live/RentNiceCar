using Microsoft.AspNet.Identity;
using RentNiceCar.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RentNiceCar.Controllers
{
    public class CarController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        public bool IsCarAvailable(Car car)
        {
            Invoice invoice = db.Invoices.OrderByDescending(x => x.InvoiceId).FirstOrDefault(x => x.Kenteken == car.Kenteken);
            if (invoice == null) return true;
            if (!invoice.InleverDatum.HasValue) return true;
            return false;
        }

        // GET: Car
        public ActionResult Index()
        {
            var list = db.Database.SqlQuery<Car>("SELECT * FROM Cars car WHERE NOT EXISTS " +
               //"(SELECT null FROM Invoices i WHERE i.Kenteken = car.Kenteken AND ((i.VanafDatum BETWEEN @vanafDatum AND @totDatum) OR (i.TotDatum BETWEEN @vanafDatum AND @totDatum)))",
               //"(SELECT null FROM Invoices i WHERE i.Kenteken = car.Kenteken AND ((@vanafDatum BETWEEN i.VanafDatum AND i.TotDatum) OR (@totDatum BETWEEN i.VanafDatum AND i.TotDatum)))",
               "(SELECT null FROM Invoices i WHERE i.Kenteken = car.Kenteken AND ((@vanafDatum >= i.VanafDatum AND @vanafDatum <= i.TotDatum) OR (@totDatum >= i.VanafDatum AND @totDatum <= i.TotDatum)))",
               new SqlParameter("vanafDatum", DateTime.Now.Date),
               new SqlParameter("totDatum", DateTime.Now.AddDays(1).Date)
           ).ToList();

            return View(new ViewCarsModel()
            {
                Cars = list,
                NuBeschikbaar= true,
                TotDatum = null,
                VanafDatum = null
            });
            //return View(db.Cars.Where(x => IsCarAvailable(x)).ToList());
        }

        // POST: Car
        [HttpPost]
        public ActionResult Index(ViewCarsModel model)
        {
            List<Car> list = new List<Car>();
            if (model.NuBeschikbaar)
            {
                list = db.Database.SqlQuery<Car>("SELECT * FROM Cars car WHERE NOT EXISTS " +
                   "(SELECT null FROM Invoices i WHERE i.Kenteken = car.Kenteken AND ((@vanafDatum >= i.VanafDatum AND @vanafDatum <= i.TotDatum) OR (@totDatum >= i.VanafDatum AND @totDatum <= i.TotDatum)))",
                   new SqlParameter("vanafDatum", DateTime.Now.Date),
                   new SqlParameter("totDatum", DateTime.Now.AddDays(1).Date)
               ).ToList();
            }
            else
            {
                if(!model.VanafDatum.HasValue || !model.TotDatum.HasValue)
                {
                    model.Error = "You must select a date range!";
                }
                else if (model.VanafDatum.Value.Date < DateTime.Now.Date)
                {
                    //Je kan alleen in de toekomst bestellen!
                    model.Error = "U probeert een bestelling in het verleden te maken!";
                }
                else if (model.TotDatum.Value.Date < model.VanafDatum.Value.Date)
                {
                    //Hoger dan vanafdatum
                    model.Error = "De datum van het terugbrengen moet hoger zijn dan de vanaf wanneer datum!";
                }
                else if ((model.TotDatum.Value.Date - model.VanafDatum.Value.Date).Days < 1)
                {
                    //Minimaal een dag
                    model.Error = "Uw bestelling moet minstens 1 dag of hoger zijn";
                }
                else
                {
                    list = db.Database.SqlQuery<Car>("SELECT * FROM Cars car WHERE NOT EXISTS " +
                       "(SELECT null FROM Invoices i WHERE i.Kenteken = car.Kenteken AND ((@vanafDatum >= i.VanafDatum AND @vanafDatum <= i.TotDatum) OR (@totDatum >= i.VanafDatum AND @totDatum <= i.TotDatum)))",
                       new SqlParameter("vanafDatum", model.VanafDatum.Value.Date),
                       new SqlParameter("totDatum", model.TotDatum.Value.Date)
                   ).ToList();
                }
            }            
            model.Cars = list;
            return View(model);
        }

        // GET: Car/Details/5
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        // GET: Car/Order/5
        [Authorize]
        public ActionResult Order(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            OrderCarViewModel orderViewModel = new OrderCarViewModel()
            {
                Kenteken = car.Kenteken
            };
            return View(orderViewModel);
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Order(OrderCarViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            model.Error = "";
            Car car = db.Cars.Find(model.Kenteken);
            if (car == null)
            {
                return HttpNotFound();
            }
            Invoice invoice = null;

            /*if(invoice != null && !invoice.InleverDatum.HasValue)
            {
                //terug HttpNotFound();
            }else */
            if (model.VanafDatum.Date < DateTime.Now.Date)
            {
                //Je kan alleen in de toekomst bestellen!
                model.Error = "U probeert een bestelling in het verleden te maken!";
            }
            else if(model.TotDatum.Date < model.VanafDatum.Date)
            {
                //Hoger dan vanafdatum
                model.Error = "De datum van het terugbrengen moet hoger zijn dan de vanaf wanneer datum!";
            }
            else if((model.TotDatum.Date - model.VanafDatum.Date).Days < 1)
            {
                //Minimaal een dag
                model.Error = "Uw bestelling moet minstens 1 dag of hoger zijn";
            }else if(db.Invoices.OrderByDescending(x => x.InvoiceId).FirstOrDefault(x => x.Kenteken == car.Kenteken &&
            //((@vanafDatum >= i.VanafDatum AND @vanafDatum <= i.TotDatum) OR (@totDatum >= i.VanafDatum AND @totDatum <= i.TotDatum))
            ((x.VanafDatum >= model.VanafDatum && x.VanafDatum <= model.TotDatum) || (x.TotDatum >= model.VanafDatum && x.TotDatum <= model.TotDatum))) != null)
            {
                //De auto is niet beschikbaar.
                model.Error = "Deze auto is helaas al vehuurd!";
            }
            else
            {
                //Alles gecontrolleerd??
                invoice = db.Invoices.Add(new Invoice()
                {
                    Kenteken = car.Kenteken,
                    InleverDatum = null,
                    VanafDatum = model.VanafDatum.Date,
                    TotDatum = model.TotDatum.Date,
                    UserId = User.Identity.GetUserId(),
                    Datum = DateTime.Now.Date
                });
                db.SaveChanges();
                //Doorverwijzen naar naar de factuur.
                return RedirectToAction("View/"+invoice.InvoiceId.ToString(), "Invoice");
            }
            return View(model);
        }

        // GET: Car/Create
        [Authorize(Roles = "Eigenaar")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Car/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Eigenaar")]
        public ActionResult Create([Bind(Include = "Kenteken,Merk,Type,Soort,Omschrijving,GPS,Prijs,Borg")] Car car)
        {
            if (ModelState.IsValid)
            {
                db.Cars.Add(car);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(car);
        }

        // GET: Car/Edit/5
        [Authorize(Roles = "Eigenaar")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        // POST: Car/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Eigenaar")]
        public ActionResult Edit([Bind(Include = "Kenteken,Merk,Type,Soort,Omschrijving,GPS,Prijs,Borg")] Car car)
        {
            if (ModelState.IsValid)
            {
                db.Entry(car).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(car);
        }

        // GET: Car/Delete/5
        [Authorize(Roles = "Eigenaar")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        // POST: Car/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Eigenaar")]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Index");
            Car car = db.Cars.Find(id);
            db.Cars.Remove(car);
            //
            List<Invoice> invoices = db.Invoices.Where(x => x.Kenteken == id).ToList();
            foreach(var invoice in invoices)
            {
                db.Invoices.Remove(invoice);
            }
            //
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
