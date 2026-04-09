import { TestBed } from '@angular/core/testing';

import { IdentityUser } from './identity-user';

describe('IdentityUser', () => {
  let service: IdentityUser;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(IdentityUser);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
