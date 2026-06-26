import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { QuillModule } from 'ngx-quill';
import { EntriesService } from '../entries.service';
import { ProjectService } from '../../../core/services/project.service';
import { SubscriberService } from '../../subscribers/subscriber.service';
import { ToastService } from '../../../core/services/toast.service';
import { parseApiError } from '../../../core/http/parse-api-error';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { Entry } from '../../../core/models/entry.model';

@Component({
  selector: 'app-entry-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, QuillModule, ConfirmDialog],
  templateUrl: './entry-editor.html',
  styleUrl: './entry-editor.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EntryEditor implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private entriesService = inject(EntriesService);
  private projectService = inject(ProjectService);
  private subscriberService = inject(SubscriberService);
  private toast = inject(ToastService);

  entryId = signal<string | null>(null);
  projectId = signal<string | null>(null);
  currentEntry = signal<Entry | null>(null);
  saving = signal(false);
  loading = signal(true);
  tagInput = signal('');

  // Publish dialog state
  showPublishDialog = signal(false);
  subscriberCount = signal<number | null>(null);
  publishing = signal(false);

  form = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(1)]],
    contentHtml: [''],
    version: [''],
    tags: [[] as string[]],
  });

  quillModules = {
    toolbar: [
      ['bold', 'italic', 'strike'],
      [{ header: [2, 3, false] }],
      [{ list: 'ordered' }, { list: 'bullet' }],
      ['code-block', 'link'],
      ['clean'],
    ],
  };

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    this.entryId.set(id);

    const project = this.projectService.activeProject();
    if (project) this.projectId.set(project.id);

    if (id) {
      this.entriesService.getById(id).subscribe({
        next: entry => {
          if (entry) {
            this.currentEntry.set(entry);
            this.form.patchValue({
              title: entry.title,
              contentHtml: entry.contentHtml,
              version: entry.version ?? '',
              tags: entry.tags,
            });
          }
          this.loading.set(false);
        },
        error: () => { this.loading.set(false); this.toast.error('Failed to load entry.'); },
      });
    } else {
      this.loading.set(false);
    }
  }

  get tags(): string[] {
    return this.form.value.tags ?? [];
  }

  get isPublished(): boolean {
    return this.currentEntry()?.status === 'Published';
  }

  onTagKeydown(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === ',') {
      event.preventDefault();
      this.addTag();
    }
  }

  addTag() {
    const val = this.tagInput().trim();
    if (!val) return;
    const current = this.tags;
    if (!current.includes(val)) {
      this.form.patchValue({ tags: [...current, val] });
    }
    this.tagInput.set('');
  }

  removeTag(tag: string) {
    this.form.patchValue({ tags: this.tags.filter(t => t !== tag) });
  }

  saveDraft() {
    if (this.form.invalid || !this.projectId()) return;
    this.saving.set(true);

    const { title, contentHtml, version, tags } = this.form.value;
    const id = this.entryId();

    if (id) {
      this.entriesService.update(id, {
        id,
        title: title!,
        contentHtml: contentHtml ?? '',
        version: version ?? undefined,
        tags: tags ?? [],
      }).subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.success('Draft saved.');
        },
        error: (err) => { this.saving.set(false); this.toast.error(parseApiError(err)); },
      });
    } else {
      this.entriesService.create({
        projectId: this.projectId()!,
        title: title!,
        contentHtml: contentHtml ?? '',
        version: version ?? undefined,
        tags: tags ?? [],
      }).subscribe({
        next: newId => {
          this.saving.set(false);
          this.entryId.set(newId);
          this.toast.success('Draft created.');
          this.router.navigate(['/app/entries', newId, 'edit'], { replaceUrl: true });
        },
        error: (err) => { this.saving.set(false); this.toast.error(parseApiError(err)); },
      });
    }
  }

  openPublishDialog() {
    const pid = this.projectId();
    if (!pid) return;

    // Load subscriber count before opening the dialog
    this.subscriberCount.set(null);
    this.showPublishDialog.set(true);

    this.subscriberService.getCount(pid).subscribe({
      next: count => this.subscriberCount.set(count),
      error: () => this.subscriberCount.set(0),
    });
  }

  closePublishDialog() {
    this.showPublishDialog.set(false);
    this.subscriberCount.set(null);
  }

  confirmPublish() {
    const id = this.entryId();
    if (!id) return;
    this.publishing.set(true);

    this.entriesService.publish(id).subscribe({
      next: () => {
        this.publishing.set(false);
        this.toast.success('Entry published successfully.');
        this.router.navigate(['/app/entries']);
      },
      error: (err) => { this.publishing.set(false); this.toast.error(parseApiError(err)); },
    });
  }
}
