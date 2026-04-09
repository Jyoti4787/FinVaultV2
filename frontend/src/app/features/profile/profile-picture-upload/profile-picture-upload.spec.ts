import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfilePictureUpload } from './profile-picture-upload';

describe('ProfilePictureUpload', () => {
  let component: ProfilePictureUpload;
  let fixture: ComponentFixture<ProfilePictureUpload>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfilePictureUpload],
    }).compileComponents();

    fixture = TestBed.createComponent(ProfilePictureUpload);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
