import { Component } from '@angular/core';
import { BuildsListComponent } from './builds-list.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [BuildsListComponent],
  template: `<app-builds-list></app-builds-list>`
})
export class AppComponent {}
