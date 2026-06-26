import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProjectService } from '../../../core/services/project.service';
import { ToastService } from '../../../core/services/toast.service';
import { parseApiError } from '../../../core/http/parse-api-error';

@Component({
  selector: 'app-project-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './project-settings.html',
  styleUrl: './project-settings.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectSettings implements OnInit {
  projectService = inject(ProjectService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  private toast = inject(ToastService);

  isNewMode = signal(false);

  saving = signal(false);
  saved = signal(false);
  deleting = signal(false);

  form = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    slug: new FormControl('', [Validators.required, Validators.maxLength(100), Validators.pattern(/^[a-z0-9-]+$/)]),
    customDomain: new FormControl<string | null>(null),
  });

  constructor() {
    effect(() => {
      const p = this.projectService.activeProject();
      if (p && !this.isNewMode()) {
        this.form.patchValue({ name: p.name, slug: p.slug, customDomain: p.customDomain ?? null }, { emitEvent: false });
        this.saved.set(false);
        this.saving.set(false);
      }
    });
  }

  ngOnInit() {
    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const newMode = params.get('mode') === 'new';
      this.isNewMode.set(newMode);
      if (newMode) {
        this.form.reset({ name: '', slug: '', customDomain: null });
      }
    });

    this.form.controls.name.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(name => {
      if (this.isNewMode() || !this.projectService.activeProject()) {
        this.form.controls.slug.setValue(this.slugify(name ?? ''), { emitEvent: false });
      }
    });
  }

  slugify(name: string): string {
    return name.trim().toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
  }

  save() {
    if (this.form.invalid || this.saving()) return;
    const { name, slug, customDomain } = this.form.getRawValue();
    const active = this.projectService.activeProject();
    this.saving.set(true);

    const obs$ = active && !this.isNewMode()
      ? this.projectService.update(active.id, {
          id: active.id,
          name: name!,
          slug: slug!,
          accentColor: active.accentColor,
          widgetPosition: active.widgetPosition,
          isPublic: active.isPublic,
          customDomain: customDomain ?? undefined,
        })
      : this.projectService.create({
          name: name!,
          slug: slug!,
          accentColor: '#6366f1',
          widgetPosition: 'br',
          isPublic: true,
        });

    obs$.subscribe({
      next: () => {
        this.saving.set(false);
        if (this.isNewMode()) {
          this.toast.success('Project created successfully.');
          this.router.navigate(['/app/dashboard']);
        } else {
          this.toast.success('Project settings saved.');
          this.saved.set(true);
          setTimeout(() => this.saved.set(false), 2000);
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.toast.error(parseApiError(err));
      },
    });
  }

  delete() {
    const active = this.projectService.activeProject();
    if (!active || this.deleting()) return;
    const name = active.name;
    this.deleting.set(true);
    this.projectService.delete(active.id).subscribe({
      next: () => {
        this.toast.success(`"${name}" has been deleted.`);
        const hasRemaining = this.projectService.projects().length > 0;
        if (hasRemaining) {
          this.router.navigate(['/app/settings/project']);
        } else {
          this.router.navigate(['/app/settings/project'], { queryParams: { mode: 'new' } });
        }
      },
      error: (err) => { this.deleting.set(false); this.toast.error(parseApiError(err)); },
    });
  }
}
