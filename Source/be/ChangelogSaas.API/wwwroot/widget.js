(function () {
  'use strict';

  // --- Read config from script tag ---
  var script = document.currentScript || (function () {
    var all = document.querySelectorAll('script[data-project]');
    return all[all.length - 1];
  })();

  if (!script) return;

  var slug = script.getAttribute('data-project');
  if (!slug) return;

  var apiBase = script.getAttribute('data-api') || (function () {
    try { return new URL(script.src).origin; } catch (e) { return ''; }
  })();

  var publicBase = script.getAttribute('data-public') || apiBase;

  // --- Unread tracking ---
  var STORAGE_KEY = 'clw_seen_' + slug;

  function getSeenIds() {
    try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]'); } catch (e) { return []; }
  }

  function markAllSeen(entries) {
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(entries.map(function (e) { return e.id; }))); } catch (e) {}
  }

  // --- Helpers ---
  function formatDate(iso) {
    try {
      var d = new Date(iso);
      return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    } catch (e) { return iso; }
  }

  var TAG_COLORS = {
    'new feature': { bg: '#eff6ff', color: '#2563eb' },
    'bug fix':     { bg: '#f0fdf4', color: '#16a34a' },
    'improvement': { bg: '#fffbeb', color: '#d97706' },
    'security':    { bg: '#fef2f2', color: '#dc2626' },
    'performance': { bg: '#f5f3ff', color: '#7c3aed' },
  };

  function tagStyle(tag) {
    var t = (tag || '').toLowerCase();
    var c = TAG_COLORS[t] || { bg: '#f5f5f4', color: '#78716c' };
    return 'background:' + c.bg + ';color:' + c.color + ';';
  }

  function positionStyles(pos) {
    var bottom = 'bottom:20px;', top = 'top:20px;', right = 'right:20px;', left = 'left:20px;';
    switch (pos) {
      case 'tl': return top + left;
      case 'tr': return top + right;
      case 'bl': return bottom + left;
      default:   return bottom + right; // br (default)
    }
  }

  function popupPositionStyles(pos) {
    var base = 'position:fixed;z-index:2147483647;width:300px;max-height:480px;display:flex;flex-direction:column;';
    switch (pos) {
      case 'tl': return base + 'top:80px;left:20px;';
      case 'tr': return base + 'top:80px;right:20px;';
      case 'bl': return base + 'bottom:80px;left:20px;';
      default:   return base + 'bottom:80px;right:20px;';
    }
  }

  // --- Inject base styles ---
  var style = document.createElement('style');
  style.textContent = [
    '#clw-fab{position:fixed;z-index:2147483646;display:flex;align-items:center;gap:6px;padding:8px 14px;border-radius:8px;border:none;cursor:pointer;font-size:13px;font-weight:500;font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;box-shadow:0 2px 12px rgba(0,0,0,.15);transition:opacity .15s,transform .15s;}',
    '#clw-fab:hover{opacity:.9;transform:scale(1.03);}',
    '#clw-fab .clw-badge{background:#ef4444;color:#fff;font-size:10px;font-weight:700;padding:1px 5px;border-radius:100px;line-height:1.4;}',
    '#clw-popup{border-radius:10px;background:#fff;border:0.5px solid #e7e5e4;box-shadow:0 8px 32px rgba(0,0,0,.12);overflow:hidden;font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;transition:opacity .15s,transform .15s;}',
    '#clw-popup.clw-hidden{opacity:0;pointer-events:none;transform:translateY(8px);}',
    '.clw-header{display:flex;align-items:center;justify-content:space-between;padding:12px 14px;border-bottom:0.5px solid #f0eeed;}',
    '.clw-header-title{font-size:13px;font-weight:500;color:#1c1917;display:flex;align-items:center;gap:6px;}',
    '.clw-close{background:none;border:none;font-size:18px;line-height:1;color:#a8a29e;cursor:pointer;padding:0;}',
    '.clw-close:hover{color:#1c1917;}',
    '.clw-entries{overflow-y:auto;flex:1;}',
    '.clw-entry{padding:12px 14px;border-bottom:0.5px solid #f0eeed;cursor:default;}',
    '.clw-entry:last-child{border-bottom:none;}',
    '.clw-entry-top{display:flex;align-items:center;gap:6px;margin-bottom:4px;flex-wrap:wrap;}',
    '.clw-tag{font-size:10px;padding:2px 6px;border-radius:4px;font-weight:500;}',
    '.clw-entry-title{font-size:13px;font-weight:500;color:#1c1917;margin:0 0 3px;}',
    '.clw-entry-date{font-size:11px;color:#a8a29e;}',
    '.clw-entry-desc{font-size:12px;color:#78716c;line-height:1.5;margin-top:4px;display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden;}',
    '.clw-footer{padding:10px 14px;border-top:0.5px solid #f0eeed;text-align:center;}',
    '.clw-footer a{font-size:12px;color:#2563eb;text-decoration:none;}',
    '.clw-footer a:hover{text-decoration:underline;}',
  ].join('');
  document.head.appendChild(style);

  // --- Fetch and render ---
  fetch(apiBase + '/api/widget/' + slug)
    .then(function (r) { if (!r.ok) throw new Error('Not found'); return r.json(); })
    .then(function (data) {
      var entries = data.entries || [];
      var accent = data.accentColor || '#6366f1';
      var pos = (data.widgetPosition || 'br').toLowerCase();
      var seen = getSeenIds();
      var unread = entries.filter(function (e) { return seen.indexOf(e.id) === -1; }).length;

      // FAB
      var fab = document.createElement('button');
      fab.id = 'clw-fab';
      fab.style.cssText = positionStyles(pos) + 'background:' + accent + ';color:#fff;';
      fab.innerHTML = '📣 What\'s new' + (unread > 0 ? ' <span class="clw-badge">' + unread + '</span>' : '');

      // Popup
      var popup = document.createElement('div');
      popup.id = 'clw-popup';
      popup.className = 'clw-hidden';
      popup.style.cssText = popupPositionStyles(pos);

      var badgeHtml = unread > 0 ? ' <span class="clw-badge" style="background:#ef4444;color:#fff;font-size:10px;font-weight:700;padding:1px 5px;border-radius:100px;">' + unread + '</span>' : '';
      var entriesHtml = entries.slice(0, 5).map(function (e) {
        var tagsHtml = (e.tags || []).map(function (t) {
          return '<span class="clw-tag" style="' + tagStyle(t) + '">' + t + '</span>';
        }).join('');
        var plain = e.contentHtml.replace(/<[^>]+>/g, '');
        return '<div class="clw-entry">'
          + '<div class="clw-entry-top">' + tagsHtml + '</div>'
          + '<div class="clw-entry-title">' + e.title + '</div>'
          + '<div class="clw-entry-date">' + formatDate(e.publishedAt) + '</div>'
          + (plain ? '<div class="clw-entry-desc">' + plain + '</div>' : '')
          + '</div>';
      }).join('');

      popup.innerHTML = '<div class="clw-header">'
        + '<span class="clw-header-title">What\'s new' + badgeHtml + '</span>'
        + '<button class="clw-close" id="clw-close-btn">×</button>'
        + '</div>'
        + '<div class="clw-entries">' + entriesHtml + '</div>'
        + '<div class="clw-footer"><a href="' + publicBase + '/c/' + slug + '" target="_blank" rel="noopener">View all updates →</a></div>';

      document.body.appendChild(fab);
      document.body.appendChild(popup);

      var open = false;

      function openPopup() {
        open = true;
        popup.classList.remove('clw-hidden');
        markAllSeen(entries);
        // Remove badge after opening
        var badge = fab.querySelector('.clw-badge');
        if (badge) badge.remove();
        var popBadge = popup.querySelector('.clw-badge');
        if (popBadge) popBadge.remove();
      }

      function closePopup() {
        open = false;
        popup.classList.add('clw-hidden');
      }

      fab.addEventListener('click', function () {
        open ? closePopup() : openPopup();
      });

      document.getElementById('clw-close-btn').addEventListener('click', closePopup);

      // Close on outside click
      document.addEventListener('click', function (e) {
        if (open && e.target !== fab && !popup.contains(e.target)) closePopup();
      });
    })
    .catch(function () { /* silent fail — don't break host app */ });
})();
