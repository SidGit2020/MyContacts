using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyContacts.Data;
using MyContacts.Models;

namespace MyContacts.Controllers;

public class ContactsController : Controller
{
    private readonly AppDbContext _db;

    public ContactsController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Contacts
    public IActionResult Index()
    {
        var contacts = _db.Contacts.ToList();
        ViewBag.UpcomingBirthdays = GetUpcomingBirthdays(contacts);
        return View(contacts);
        //Test Git Update to remote
        //Test commit to Git 2
    }

    // GET: /Contacts/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Contacts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Contact contact)
    {
        if (!ModelState.IsValid)
            return View(contact);

        _db.Contacts.Add(contact);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Contacts/Details/5
    public IActionResult Details(int id)
    {
        var contact = _db.Contacts.Find(id);
        if (contact == null) return NotFound();
        return View(contact);
    }

    // GET: /Contacts/Edit/5
    public IActionResult Edit(int id)
    {
        var contact = _db.Contacts.Find(id);
        if (contact == null) return NotFound();
        return View(contact);
    }

    // POST: /Contacts/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Contact contact)
    {
        if (id != contact.Id) return NotFound();

        if (!ModelState.IsValid)
            return View(contact);

        _db.Contacts.Update(contact);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Contacts/Delete/5
    public IActionResult Delete(int id)
    {
        var contact = _db.Contacts.Find(id);
        if (contact == null) return NotFound();
        return View(contact);
    }

    // POST: /Contacts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var contact = _db.Contacts.Find(id);
        if (contact != null)
        {
            _db.Contacts.Remove(contact);
            _db.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }

    private List<(Contact contact, int daysUntil)> GetUpcomingBirthdays(IEnumerable<Contact> contacts)
    {
        var today = DateTime.Today;
        var result = new List<(Contact contact, int daysUntil)>();

        foreach (var contact in contacts)
        {
            if (!contact.DateOfBirth.HasValue) continue;

            var dob = contact.DateOfBirth.Value;

            if (dob.Month == 2 && dob.Day == 29)
            {
                if (DateTime.IsLeapYear(today.Year))
                {
                    var leapBirthday = new DateTime(today.Year, 2, 29);
                    var leapDays = (leapBirthday - today).Days;
                    if (leapDays >= 0 && leapDays <= 6)
                        result.Add((contact, leapDays));
                }
                continue;
            }

            var thisYear = new DateTime(today.Year, dob.Month, dob.Day);
            if (thisYear < today) thisYear = thisYear.AddYears(1);
            var days = (thisYear - today).Days;

            if (days >= 0 && days <= 6)
                result.Add((contact, days));
        }

        return result.OrderBy(x => x.daysUntil).ToList();
    }
}
