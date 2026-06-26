import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from '../http/api.service';
import {
  CreateProjectRequest,
  GetProjectsRequest,
  Project,
  UpdateProjectRequest,
} from '../models/project.model';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private api = inject(ApiService);

  projects = signal<Project[]>([]);
  activeProject = signal<Project | null>(null);

  loadProjects(): Observable<Project[]> {
    return this.api.get<Project[]>('/projects').pipe(
      tap(list => {
        this.projects.set(list);
        const currentId = this.activeProject()?.id;
        const synced = currentId ? (list.find(p => p.id === currentId) ?? list[0]) : list[0];
        if (synced) this.activeProject.set(synced ?? null);
      })
    );
  }

  setActive(project: Project): void {
    this.activeProject.set(project);
  }

  getAll(params?: GetProjectsRequest): Observable<Project[]> {
    const query = new URLSearchParams();
    if (params?.name) query.set('name', params.name);
    if (params?.slug) query.set('slug', params.slug);
    const qs = query.toString();
    return this.api.get<Project[]>(`/projects${qs ? '?' + qs : ''}`);
  }

  create(request: CreateProjectRequest): Observable<Project> {
    return this.api.post<Project>('/projects', request).pipe(
      tap(newProject => {
        this.projects.update(list => [...list, newProject]);
        this.activeProject.set(newProject);
      })
      
    );
  }

  update(id: string, request: UpdateProjectRequest): Observable<Project> {
    return this.api.put<Project>(`/projects/${id}`, request).pipe(
      tap(updated => {
        this.projects.update(list => list.map(p => p.id === id ? updated : p));
        if (this.activeProject()?.id === id) this.activeProject.set(updated);
      })
    );
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`/projects/${id}`).pipe(
      tap(() => {
        const remaining = this.projects().filter(p => p.id !== id);
        this.projects.set(remaining);
        if (this.activeProject()?.id === id) {
          this.activeProject.set(remaining[0] ?? null);
        }
      })
    );
  }
}
