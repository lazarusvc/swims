using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;

namespace SWIMS.Controllers
{
    public class formTableDataController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public formTableDataController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        // GET: formTableData
        public async Task<IActionResult> Index()
        {
            var swimsDb_moreContext = _context.SW_formTableData.Include(s => s.SW_forms);
            return View(await swimsDb_moreContext.ToListAsync());
        }

        // GET: formTableData/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formTableDatum = await _context.SW_formTableData
                .Include(s => s.SW_forms)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formTableDatum == null)
            {
                return NotFound();
            }

            return View(sW_formTableDatum);
        }

        // GET: formTableData/Create
        public IActionResult Create()
        {
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name");
            return View();
        }

        // POST: formTableData/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FormData01,FormData02,FormData03,FormData04,FormData05,FormData06,FormData07,FormData08,FormData09,FormData10,FormData11,FormData12,FormData13,FormData14,FormData15,FormData16,FormData17,FormData18,FormData19,FormData20,FormData21,FormData22,FormData23,FormData24,FormData25,FormData26,FormData27,FormData28,FormData29,FormData30,FormData31,FormData32,FormData33,FormData34,FormData35,FormData36,FormData37,FormData38,FormData39,FormData40,FormData41,FormData42,FormData43,FormData44,FormData45,FormData46,FormData47,FormData48,FormData49,FormData50,FormData51,FormData52,FormData53,FormData54,FormData55,FormData56,FormData57,FormData58,FormData59,FormData60,FormData61,FormData62,FormData63,FormData64,FormData65,FormData66,FormData67,FormData68,FormData69,FormData70,FormData71,FormData72,FormData73,FormData74,FormData75,FormData76,FormData77,FormData78,FormData79,FormData80,FormData81,FormData82,FormData83,FormData84,FormData85,FormData86,FormData87,FormData88,FormData89,FormData90,FormData91,FormData92,FormData93,FormData94,FormData95,FormData96,FormData97,FormData98,FormData99,FormData100,FormData101,FormData102,FormData103,FormData104,FormData105,FormData106,FormData107,FormData108,FormData109,FormData110,FormData111,FormData112,FormData113,FormData114,FormData115,FormData116,FormData117,FormData118,FormData119,FormData120,FormData121,FormData122,FormData123,FormData124,FormData125,FormData126,FormData127,FormData128,FormData129,FormData130,FormData131,FormData132,FormData133,FormData134,FormData135,FormData136,FormData137,FormData138,FormData139,FormData140,FormData141,FormData142,FormData143,FormData144,FormData145,FormData146,FormData147,FormData148,FormData149,FormData150,FormData151,FormData152,FormData153,FormData154,FormData155,FormData156,FormData157,FormData158,FormData159,FormData160,FormData161,FormData162,FormData163,FormData164,FormData165,FormData166,FormData167,FormData168,FormData169,FormData170,FormData171,FormData172,FormData173,FormData174,FormData175,FormData176,FormData177,FormData178,FormData179,FormData180,FormData181,FormData182,FormData183,FormData184,FormData185,FormData186,FormData187,FormData188,FormData189,FormData190,FormData191,FormData192,FormData193,FormData194,FormData195,FormData196,FormData197,FormData198,FormData199,FormData200,FormData201,FormData202,FormData203,FormData204,FormData205,FormData206,FormData207,FormData208,FormData209,FormData210,FormData211,FormData212,FormData213,FormData214,FormData215,FormData216,FormData217,FormData218,FormData219,FormData220,FormData221,FormData222,FormData223,FormData224,FormData225,FormData226,FormData227,FormData228,FormData229,FormData230,FormData231,FormData232,FormData233,FormData234,FormData235,FormData236,FormData237,FormData238,FormData239,FormData240,FormData241,FormData242,FormData243,FormData244,FormData245,FormData246,FormData247,FormData248,FormData249,FormData250,SW_formsId,isApproval_01,isApproval_02,isApproval_03,isAppComment_01,isAppComment_02,isAppComment_03,isApprover_01,isApprover_02,isApprover_03,isLinkingForm")] SW_formTableDatum sW_formTableDatum)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_formTableDatum);
                await _context.SaveChangesAsync();
                string? uID = _context.SW_forms.Where(x => x.Id == sW_formTableDatum.SW_formsId).Select(x => x.uuid).FirstOrDefault();
                return RedirectToAction("Program", "form", new { uuid = uID });
            }
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formTableDatum.SW_formsId);
            return View(sW_formTableDatum);
        }

        // GET: formTableData/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formTableDatum = await _context.SW_formTableData.FindAsync(id);
            if (sW_formTableDatum == null)
            {
                return NotFound();
            }
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formTableDatum.SW_formsId);
            return View(sW_formTableDatum);
        }

        // POST: formTableData/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FormData01,FormData02,FormData03,FormData04,FormData05,FormData06,FormData07,FormData08,FormData09,FormData10,FormData11,FormData12,FormData13,FormData14,FormData15,FormData16,FormData17,FormData18,FormData19,FormData20,FormData21,FormData22,FormData23,FormData24,FormData25,FormData26,FormData27,FormData28,FormData29,FormData30,FormData31,FormData32,FormData33,FormData34,FormData35,FormData36,FormData37,FormData38,FormData39,FormData40,FormData41,FormData42,FormData43,FormData44,FormData45,FormData46,FormData47,FormData48,FormData49,FormData50,FormData51,FormData52,FormData53,FormData54,FormData55,FormData56,FormData57,FormData58,FormData59,FormData60,FormData61,FormData62,FormData63,FormData64,FormData65,FormData66,FormData67,FormData68,FormData69,FormData70,FormData71,FormData72,FormData73,FormData74,FormData75,FormData76,FormData77,FormData78,FormData79,FormData80,FormData81,FormData82,FormData83,FormData84,FormData85,FormData86,FormData87,FormData88,FormData89,FormData90,FormData91,FormData92,FormData93,FormData94,FormData95,FormData96,FormData97,FormData98,FormData99,FormData100,FormData101,FormData102,FormData103,FormData104,FormData105,FormData106,FormData107,FormData108,FormData109,FormData110,FormData111,FormData112,FormData113,FormData114,FormData115,FormData116,FormData117,FormData118,FormData119,FormData120,FormData121,FormData122,FormData123,FormData124,FormData125,FormData126,FormData127,FormData128,FormData129,FormData130,FormData131,FormData132,FormData133,FormData134,FormData135,FormData136,FormData137,FormData138,FormData139,FormData140,FormData141,FormData142,FormData143,FormData144,FormData145,FormData146,FormData147,FormData148,FormData149,FormData150,FormData151,FormData152,FormData153,FormData154,FormData155,FormData156,FormData157,FormData158,FormData159,FormData160,FormData161,FormData162,FormData163,FormData164,FormData165,FormData166,FormData167,FormData168,FormData169,FormData170,FormData171,FormData172,FormData173,FormData174,FormData175,FormData176,FormData177,FormData178,FormData179,FormData180,FormData181,FormData182,FormData183,FormData184,FormData185,FormData186,FormData187,FormData188,FormData189,FormData190,FormData191,FormData192,FormData193,FormData194,FormData195,FormData196,FormData197,FormData198,FormData199,FormData200,FormData201,FormData202,FormData203,FormData204,FormData205,FormData206,FormData207,FormData208,FormData209,FormData210,FormData211,FormData212,FormData213,FormData214,FormData215,FormData216,FormData217,FormData218,FormData219,FormData220,FormData221,FormData222,FormData223,FormData224,FormData225,FormData226,FormData227,FormData228,FormData229,FormData230,FormData231,FormData232,FormData233,FormData234,FormData235,FormData236,FormData237,FormData238,FormData239,FormData240,FormData241,FormData242,FormData243,FormData244,FormData245,FormData246,FormData247,FormData248,FormData249,FormData250,SW_formsId,isApproval_01,isApproval_02,isApproval_03,isAppComment_01,isAppComment_02,isAppComment_03,isApprover_01,isApprover_02,isApprover_03,isLinkingForm")] SW_formTableDatum sW_formTableDatum)
        {
            if (id != sW_formTableDatum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_formTableDatum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_formTableDatumExists(sW_formTableDatum.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formTableDatum.SW_formsId);
            return View(sW_formTableDatum);
        }

        // GET: formTableData/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formTableDatum = await _context.SW_formTableData
                .Include(s => s.SW_forms)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formTableDatum == null)
            {
                return NotFound();
            }

            return View(sW_formTableDatum);
        }

        // POST: formTableData/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_formTableDatum = await _context.SW_formTableData.FindAsync(id);
            if (sW_formTableDatum != null)
            {
                _context.SW_formTableData.Remove(sW_formTableDatum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_formTableDatumExists(int id)
        {
            return _context.SW_formTableData.Any(e => e.Id == id);
        }
    }
}
