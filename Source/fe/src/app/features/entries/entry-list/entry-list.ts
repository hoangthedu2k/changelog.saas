import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EntriesService } from '../entries.service';
import { ProjectService } from '../../../core/services/project.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { Entry } from '../../../core/models/entry.model';

@Component({
  selector: 'app-entry-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ConfirmDialog],
  templateUrl: './entry-list.html',
  styleUrl: './entry-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EntryList {
  projectService = inject(ProjectService);
  private entriesService = inject(EntriesService);
  private toast = inject(ToastService);

  entries = signal<Entry[]>([]);
  loading = signal(false);

  // Delete dialog
  pendingDeleteEntry = signal<Entry | null>(null);
  deleting = signal(false);

  constructor() {
    effect(() => {
      const project = this.projectService.activeProject();
      if (!project) return;

      this.loading.set(true);
      this.entriesService.getAll({ projectId: project.id }).subscribe({
        next: entries => { this.entries.set(entries); this.loading.set(false); },
        error: () => this.loading.set(false),
      });
    });
  }

  publish(entry: Entry) {
    this.entriesService.publish(entry.id).subscribe({
      next: () => {
        this.entries.update(list =>
          list.map(e => e.id === entry.id ? { ...e, status: 'Published' as const } : e)
        );
        this.toast.success(`"${entry.title}" published.`);
      },
      error: () => this.toast.error('Failed to publish entry.'),
    });
  }

  openDeleteDialog(entry: Entry) {
    this.pendingDeleteEntry.set(entry);
  }

  cancelDelete() {
    this.pendingDeleteEntry.set(null);
  }

  confirmDelete() {
    const entry = this.pendingDeleteEntry();
    if (!entry) return;
    const title = entry.title;
    this.deleting.set(true);
    this.entriesService.delete(entry.id).subscribe({
      next: () => {
        this.entries.update(list => list.filter(e => e.id !== entry.id));
        this.pendingDeleteEntry.set(null);
        this.deleting.set(false);
        this.toast.success(`"${title}" has been deleted.`);
      },
      error: () => this.deleting.set(false),
    });
  }
}
