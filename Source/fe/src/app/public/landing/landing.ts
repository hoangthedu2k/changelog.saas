import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './landing.html',
  styleUrl: './landing.scss',
})
export class Landing {
  readonly steps = [
    {
      number: '01',
      title: 'Create your project',
      description:
        'Sign up and create a changelog project. Give it a name and a unique slug that becomes your public URL.',
    },
    {
      number: '02',
      title: 'Publish updates',
      description:
        'Write release notes using the rich editor. Tag entries as Feature, Fix, or Improvement and publish instantly.',
    },
    {
      number: '03',
      title: 'Share with your users',
      description:
        'Share your public changelog URL or embed the widget directly into your app — no extra setup needed.',
    },
    {
      number: '04',
      title: 'Grow your audience',
      description:
        'Users subscribe with their email. Every new entry triggers an automatic notification to all subscribers.',
    },
  ];

  readonly features = [
    {
      icon: '📝',
      title: 'Rich text editor',
      description: 'Write beautiful release notes with formatting, code blocks, and media.',
    },
    {
      icon: '🔔',
      title: 'Email notifications',
      description: 'Subscribers get notified automatically whenever you publish a new entry.',
    },
    {
      icon: '🏷️',
      title: 'Tags & filtering',
      description: 'Categorize entries with tags so users can filter by what matters to them.',
    },
    {
      icon: '🔌',
      title: 'Embeddable widget',
      description: 'Drop a lightweight widget into your app with a single script tag.',
    },
    {
      icon: '🌐',
      title: 'Public changelog page',
      description: 'Every project gets a shareable public page at your own slug URL.',
    },
    {
      icon: '⚡',
      title: 'Instant publishing',
      description: 'Changes go live immediately — no build step, no deployment needed.',
    },
  ];
}
