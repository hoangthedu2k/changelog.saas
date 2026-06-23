import { ChangeDetectionStrategy, Component, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-docs',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './docs.html',
  styleUrl: './docs.scss',
})
export class Docs {
  private platformId = inject(PLATFORM_ID);

  scrollTo(id: string): void {
    if (!isPlatformBrowser(this.platformId)) return;
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  readonly sections = [
    {
      id: 'getting-started',
      title: 'Getting Started',
      steps: [
        {
          number: '1',
          title: 'Create an account',
          body: 'Go to the <a routerLink="/register">register page</a> and sign up with your email. No credit card required.',
          code: null,
        },
        {
          number: '2',
          title: 'Create a project',
          body: 'From the dashboard, click <strong>New project</strong>. Give it a name and choose a unique slug — this becomes your public URL (e.g. <code>yourapp.changelogsaas.com/c/your-slug</code>).',
          code: null,
        },
        {
          number: '3',
          title: 'Publish your first entry',
          body: 'Go to <strong>Entries → New entry</strong>. Write your release note, pick a tag (Feature / Fix / Improvement), and hit <strong>Publish</strong>.',
          code: null,
        },
        {
          number: '4',
          title: 'Share your changelog',
          body: 'Your public changelog is live immediately at <code>/c/your-slug</code>. Share the link or embed the widget in your app.',
          code: null,
        },
      ],
    },
    {
      id: 'widget',
      title: 'Embedding the Widget',
      steps: [
        {
          number: null,
          title: 'Add the script tag',
          body: 'Paste the snippet below into your HTML, just before <code>&lt;/body&gt;</code>. Replace <code>YOUR_SLUG</code> with your project slug.',
          code: `<script
  src="https://cdn.changelogsaas.com/widget.js"
  data-slug="YOUR_SLUG"
  defer>
</script>`,
        },
        {
          number: null,
          title: 'Add the trigger element',
          body: 'Add a button or link with the <code>data-changelog-trigger</code> attribute anywhere in your UI. The widget opens as a popover.',
          code: `<button data-changelog-trigger>
  What's new
</button>`,
        },
        {
          number: null,
          title: 'Customise appearance (optional)',
          body: 'Override colors using CSS variables on the trigger element\'s parent.',
          code: `:root {
  --cl-accent: #f97316;   /* badge / highlight color */
  --cl-radius: 8px;        /* border radius */
}`,
        },
      ],
    },
    {
      id: 'entries',
      title: 'Writing Entries',
      steps: [
        {
          number: null,
          title: 'Tags',
          body: 'Each entry has one tag: <strong>Feature</strong> (new functionality), <strong>Fix</strong> (bug fix), or <strong>Improvement</strong> (enhancement to existing behaviour). Tags are shown as colored badges on your public page and in the widget.',
          code: null,
        },
        {
          number: null,
          title: 'Rich text editor',
          body: 'The editor supports <strong>bold</strong>, <em>italic</em>, headings, bullet lists, numbered lists, inline code, and code blocks. You can also paste images directly.',
          code: null,
        },
        {
          number: null,
          title: 'Draft vs Published',
          body: 'Entries saved as <strong>Draft</strong> are not visible on the public page. Set status to <strong>Published</strong> when you are ready — subscribers get an email notification automatically.',
          code: null,
        },
      ],
    },
    {
      id: 'subscribers',
      title: 'Subscribers & Notifications',
      steps: [
        {
          number: null,
          title: 'How users subscribe',
          body: 'Visitors on your public changelog page can enter their email in the subscribe form. They receive a confirmation email and are added to your subscriber list after confirming.',
          code: null,
        },
        {
          number: null,
          title: 'Automatic notifications',
          body: 'When you publish an entry, all confirmed subscribers receive an email notification containing the entry title, tag, and a link to your changelog.',
          code: null,
        },
        {
          number: null,
          title: 'Managing subscribers',
          body: 'View all subscribers from the <strong>Subscribers</strong> tab in the dashboard. You can see their email, subscription date, and confirmation status.',
          code: null,
        },
      ],
    },
  ];

  readonly faq = [
    {
      q: 'Is there a free plan?',
      a: 'Yes. All features are free. There is no paid tier — no subscriber limit, no entry limit.',
    },
    {
      q: 'Can I use a custom domain?',
      a: 'Custom domain support is on the roadmap. For now, your changelog is available at <code>/c/your-slug</code> on our domain.',
    },
    {
      q: 'What happens when I publish an entry?',
      a: 'The entry appears on your public changelog immediately. An email notification is queued and sent to all confirmed subscribers within a few seconds.',
    },
    {
      q: 'Can I delete subscribers?',
      a: 'Subscribers can unsubscribe at any time via the link in each email. You can also view and remove them from the Subscribers page in your dashboard.',
    },
  ];
}
