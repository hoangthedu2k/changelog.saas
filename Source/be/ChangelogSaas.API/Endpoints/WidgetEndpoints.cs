using ChangelogSaas.Application.Widget.Queries.GetWidgetByDomainQuery;
using ChangelogSaas.Application.Widget.Queries.GetWidgetQuery;
using MediatR;

namespace ChangelogSaas.API.Endpoints
{
    public static class WidgetEndpoints
    {
        public static IEndpointRouteBuilder MapWidgetEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/widget/{slug}", async (string slug, ISender sender, CancellationToken ct)
                => Results.Ok(await sender.Send(new GetWidgetQuery(slug), ct)))
               .RequireCors("Widget")
               .WithTags("Widget");

            // Resolve widget data by custom domain — reads Host header
            app.MapGet("/api/widget/by-domain", async (HttpContext ctx, ISender sender, CancellationToken ct) =>
            {
                var host = ctx.Request.Host.Value ?? "";
                return Results.Ok(await sender.Send(new GetWidgetByDomainQuery(host), ct));
            }).RequireCors("Widget")
              .WithTags("Widget");

            // Embeddable widget script
            app.MapGet("/api/widget.js", (HttpContext ctx) =>
            {
                var scheme = ctx.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? ctx.Request.Scheme;
                var origin = $"{scheme}://{ctx.Request.Host}";
                var js = $$"""
(function () {
  var script = document.currentScript || (function () {
    var scripts = document.getElementsByTagName('script');
    return scripts[scripts.length - 1];
  })();
  var slug = script.getAttribute('data-project');
  var publicUrl = script.getAttribute('data-public') || '';
  var apiBase = '{{origin}}';
  if (!slug) return;

  var style = document.createElement('style');
  style.textContent = [
    '#_hcl-btn{position:fixed;bottom:24px;right:24px;z-index:99999;background:#6c63ff;color:#fff;border:none;border-radius:999px;padding:10px 18px;font-size:14px;font-family:inherit;cursor:pointer;box-shadow:0 4px 14px rgba(108,99,255,.4);display:flex;align-items:center;gap:8px}',
    '#_hcl-btn:hover{background:#5a52e0}',
    '#_hcl-panel{position:fixed;bottom:76px;right:24px;z-index:99999;width:340px;max-height:480px;background:#fff;border-radius:12px;box-shadow:0 8px 32px rgba(0,0,0,.15);overflow:hidden;display:none;flex-direction:column;font-family:inherit}',
    '#_hcl-panel.open{display:flex}',
    '#_hcl-header{padding:16px 18px;border-bottom:1px solid #f0eff6;display:flex;justify-content:space-between;align-items:center}',
    '#_hcl-header h3{margin:0;font-size:15px;font-weight:700;color:#1a1040}',
    '#_hcl-header a{font-size:12px;color:#6c63ff;text-decoration:none}',
    '#_hcl-list{overflow-y:auto;padding:12px 0}',
    '._hcl-item{padding:12px 18px;border-bottom:1px solid #f5f5f5}',
    '._hcl-item:last-child{border-bottom:none}',
    '._hcl-tag{display:inline-block;font-size:10px;font-weight:600;padding:2px 8px;border-radius:999px;margin-bottom:6px;text-transform:uppercase;letter-spacing:.5px}',
    '._hcl-tag.feature{background:#ede9fe;color:#5b21b6}._hcl-tag.fix{background:#dcfce7;color:#15803d}._hcl-tag.improvement{background:#dbeafe;color:#1d4ed8}._hcl-tag.other{background:#f3f4f6;color:#374151}',
    '._hcl-title{font-size:13px;font-weight:600;color:#1a1040;margin:0 0 4px}',
    '._hcl-date{font-size:11px;color:#9ca3af}',
    '#_hcl-empty{padding:32px;text-align:center;color:#9ca3af;font-size:13px}'
  ].join('');
  document.head.appendChild(style);

  var btn = document.createElement('button');
  btn.id = '_hcl-btn';
  btn.innerHTML = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M18 8h1a4 4 0 0 1 0 8h-1"/><path d="M2 8h16v9a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V8z"/><line x1="6" y1="1" x2="6" y2="4"/><line x1="10" y1="1" x2="10" y2="4"/><line x1="14" y1="1" x2="14" y2="4"/></svg> What\'s new';
  document.body.appendChild(btn);

  var panel = document.createElement('div');
  panel.id = '_hcl-panel';
  var viewAll = publicUrl ? '<a href="' + publicUrl + '/c/' + slug + '" target="_blank">View all</a>' : '';
  panel.innerHTML = '<div id="_hcl-header"><h3>What\'s new</h3>' + viewAll + '</div><div id="_hcl-list"><p id="_hcl-empty">Loading...</p></div>';
  document.body.appendChild(panel);

  var loaded = false;
  btn.addEventListener('click', function () {
    panel.classList.toggle('open');
    if (!loaded) { loaded = true; loadEntries(); }
  });

  document.addEventListener('click', function (e) {
    if (!panel.contains(e.target) && e.target !== btn) panel.classList.remove('open');
  });

  function tagClass(t) {
    if (!t) return 'other';
    var v = t.toLowerCase();
    if (v === 'feature') return 'feature';
    if (v === 'fix' || v === 'bugfix') return 'fix';
    if (v === 'improvement') return 'improvement';
    return 'other';
  }

  function loadEntries() {
    var list = document.getElementById('_hcl-list');
    fetch(apiBase + '/api/widget/' + slug)
      .then(function (r) { return r.json(); })
      .then(function (data) {
        var entries = (data.entries || []).slice(0, 8);
        if (!entries.length) { list.innerHTML = '<p id="_hcl-empty">No updates yet.</p>'; return; }
        list.innerHTML = entries.map(function (e) {
          var tag = (e.tags && e.tags[0]) || '';
          var date = e.publishedAt ? new Date(e.publishedAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) : '';
          return '<div class="_hcl-item"><span class="_hcl-tag ' + tagClass(tag) + '">' + (tag || 'Update') + '</span><p class="_hcl-title">' + e.title + '</p><p class="_hcl-date">' + date + '</p></div>';
        }).join('');
      })
      .catch(function () { list.innerHTML = '<p id="_hcl-empty">Could not load updates.</p>'; });
  }
})();
""";
                return Results.Content(js, "application/javascript");
            }).RequireCors("Widget")
              .WithTags("Widget");

            return app;
        }
    }
}
