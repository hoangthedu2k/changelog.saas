import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomDomain } from './custom-domain';

describe('CustomDomain', () => {
  let component: CustomDomain;
  let fixture: ComponentFixture<CustomDomain>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomDomain],
    }).compileComponents();

    fixture = TestBed.createComponent(CustomDomain);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
