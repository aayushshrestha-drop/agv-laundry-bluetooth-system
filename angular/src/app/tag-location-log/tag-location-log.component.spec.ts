import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TagLocationLogComponent } from './tag-location-log.component';

describe('TagLocationLogComponent', () => {
  let component: TagLocationLogComponent;
  let fixture: ComponentFixture<TagLocationLogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TagLocationLogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TagLocationLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
