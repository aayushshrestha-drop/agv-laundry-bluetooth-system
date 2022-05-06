import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TagService, TagDto, CreateUpdateTagDto } from '@proxy/tags';

@Component({
  selector: 'app-tag',
  templateUrl: './tag.component.html',
  styleUrls: ['./tag.component.scss'],
  providers: [ListService],
})
export class TagComponent implements OnInit {
  data = { items: [], totalCount: 0 } as PagedResultDto<TagDto>;
  form: FormGroup;
  selected = {} as TagDto;
  isModalOpen = false;
  constructor(public readonly list: ListService,
    private service: TagService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService) { }

  ngOnInit() {
    
    const streamCreator = (query) => this.service.getList(query);

    this.list.hookToQuery(streamCreator).subscribe((response) => {
      console.log(this.list.filter)
      this.data = response;
    });
  }
  create() {
    this.selected = {} as TagDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  edit(id: string) {
    this.service.get(id).subscribe((asset) => {
      this.selected = asset;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  buildForm() {
    this.form = this.fb.group({
      systemName: [this.selected.systemName, Validators.required],
      displayName: [this.selected.displayName, Validators.required],
      tagId: [this.selected.tagId],
      status: [this.selected.status, Validators.required],
      cartNo: [this.selected.cartNo]
    });
  }

  save() {
    if (this.form.invalid) {
      return;
    }
    const dto: CreateUpdateTagDto = {
      systemName: this.form.get("systemName").value,
      displayName: this.form.get("displayName").value,
      tagId: this.form.get("tagId").value,
      status: this.form.get("status").value,
      cartNo: this.form.get("cartNo").value,
    };
    const request = this.selected.id
      ? this.service.update(this.selected.id, dto)
      : this.service.create(dto);

    request.subscribe(() => {
      this.isModalOpen = false;
      this.form.reset();
      this.list.get();
    });
  }

  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', 'AbpAccount::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.service.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
