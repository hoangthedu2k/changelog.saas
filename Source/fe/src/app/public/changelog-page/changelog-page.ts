import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  PLATFORM_ID,
  signal,
} from '@angular/core';
import { DOCUMENT, CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { Title, Meta } from '@angular/platform-browser';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../core/http/api.service';
import { EntryCard } from '../entry-card/entry-card';
import { WidgetData } from '../../core/models/widget.model';

const ALL_TAG = '__all__';

@Component({
  selector: 'app-changelog-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, EntryCard],
  templateUrl: './changelog-page.html',
  styleUrl: './changelog-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangelogPage implements OnInit {
  private route = inject(ActivatedRoute);
  private api = inject(ApiService);
  private destroyRef = inject(DestroyRef);
  private titleService = inject(Title);
  private meta = inject(Meta);
  private doc = inject(DOCUMENT);
  private platformId = inject(PLATFORM_ID);

  data = signal<WidgetData | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  activeTag = signal(ALL_TAG);
  emailControl = new FormControl('', [Validators.required, Validators.email]);
  subscribing = signal(false);
  subscribeState = signal<'idle' | 'success' | 'error'>('idle');
  subscribeError = signal<string | null>(null);

  filteredEntries = computed(() => {
    const d = this.data();
    if (!d) return [];
    if (this.activeTag() === ALL_TAG) return d.entries;
    return d.entries.filter(e =>
      e.tags.some(t => t.toLowerCase() === this.activeTag())
    );
  });

  allTags = computed(() => {
    const d = this.data();
    if (!d) return [];
    const set = new Set<string>();
    d.entries.forEach(e => e.tags.forEach(t => set.add(t.toLowerCase())));
    return Array.from(set);
  });

  ngOnInit() {
    if (!isPlatformBrowser(this.platformId)) return;
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.api
      .get<WidgetData>(`/widget/${slug}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: data => {
          this.data.set(data);
          this.loading.set(false);
          this.applySeo(data);
        },
        error: () => {
          this.error.set('Changelog not found.');
          this.loading.set(false);
        },
      });
  }

  private applySeo(data: WidgetData) {
    const title = `${data.projectName} Changelog`;
    const description = `Updates & release notes for ${data.projectName}`;
    const url = this.doc.URL;

    this.titleService.setTitle(title);

    this.meta.updateTag({ name: 'description', content: description });

    this.meta.updateTag({ property: 'og:type', content: 'website' });
    this.meta.updateTag({ property: 'og:title', content: title });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:url', content: url });
  }

  setTag(tag: string) {
    this.activeTag.set(tag);
  }

  subscribe() {
    if (this.emailControl.invalid || this.subscribing()) return;
    const d = this.data();
    if (!d) return;

    this.subscribing.set(true);
    this.subscribeError.set(null);
    this.api
      .post<{ confirmToken: string }>('/subscribers', {
        projectId: d.projectId,
        email: this.emailControl.value,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.subscribing.set(false);
          this.subscribeState.set('success');
          this.emailControl.reset();
        },
        error: (err) => {
          this.subscribing.set(false);
          this.subscribeState.set('error');
          this.subscribeError.set(err?.error?.detail ?? 'Something went wrong, please try again.');
        },
      });
  }
}
