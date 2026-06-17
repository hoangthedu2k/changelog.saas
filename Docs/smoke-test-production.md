# Production Smoke Test Checklist

Run this after every production deployment. Check each item manually.

## 1. Health Check
- [ ] `GET https://api.horachangelog.com/api/health` returns `{"status":"ok"}`

## 2. Auth Flow
- [ ] Register a new account with email + password
- [ ] Login with that account → redirected to dashboard
- [ ] Login with Google OAuth → redirected to dashboard
- [ ] Login with Facebook OAuth → redirected to dashboard
- [ ] Logout → redirected to login page
- [ ] Accessing `/app/dashboard` without auth → redirected to login

## 3. Project
- [ ] Create a new project → appears in sidebar dropdown
- [ ] Switch between projects → dashboard data updates
- [ ] Edit project name and slug → saved correctly
- [ ] Set custom domain → saved correctly

## 4. Entries
- [ ] Create a new Draft entry (title, content, tags, version)
- [ ] Edit the entry → changes saved
- [ ] Publish the entry → status changes to Published
- [ ] Delete an entry → removed from list

## 5. Public Changelog Page
- [ ] Visit `https://horachangelog.com/c/{slug}` → page loads with published entry
- [ ] Entry card shows title, content, version, tags, date
- [ ] Page title and meta description are correct (check browser tab + View Source)

## 6. Subscriber Flow
- [ ] Subscribe with an email on the public changelog page
- [ ] Check inbox for confirmation email → click confirm link
- [ ] Subscriber appears in dashboard with status Verified
- [ ] Publish a new entry → subscriber receives notification email

## 7. Widget
- [ ] Widget setup page shows embed code with correct project slug
- [ ] Paste embed code into a plain HTML file → widget loads and shows entries

## 8. Settings — Appearance
- [ ] Change accent color → public page reflects the change
- [ ] Toggle public/private → public page accessible or returns 404

## 9. Error Tracking
- [ ] Open Sentry dashboard → no unexpected errors from smoke test
- [ ] Trigger a 404 (visit `/app/nonexistent`) → check Sentry captures it

## 10. Uptime Monitor
- [ ] UptimeRobot / Better Uptime shows monitor for `GET /api/health` as UP
- [ ] Monitor for frontend URL is UP

---

**Sign-off:** Date _________  Tester _________  All items checked: YES / NO
