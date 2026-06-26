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
        return View(contacts);
        //Test Git Update to remote
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
}
