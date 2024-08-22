import { Dto } from '../dtos';

export class ApplicationUserDto extends Dto {
  email: string;
  firstName: string;
  lastName: string;
  preferedCultureName: string;
  roles: ApplicationRoleDto[];
}

export class ApplicationRoleDto extends Dto {
  name: string;
}
