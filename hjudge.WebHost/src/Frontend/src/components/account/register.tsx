import * as React from 'react';
import { Modal, Input, Button, Form, Label } from 'semantic-ui-react';
import { SerializeForm } from '../../utils/formHelper';
import { Put } from '../../utils/requestHelper';
import { ResultModel } from '../../interfaces/resultModel';
import { CommonProps } from '../../interfaces/commonProps';

interface RegisterProps extends CommonProps {
  modalOpen: boolean,
  closeModal: (() => void)
}

export default class Register extends React.Component<RegisterProps> {
  constructor(props: RegisterProps) {
    super(props);
    this.register = this.register.bind(this);
  }

  register(event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
    let element = event.target as HTMLButtonElement;
    let form = document.querySelector('#registerForm') as HTMLFormElement;
    if (form.reportValidity()) {
      element.disabled = true;
      Put('/user/register', SerializeForm(form))
        .then(res => res.json())
        .then(data => {
          let result = data as ResultModel;
          if (result.succeeded) {
            this.props.closeModal();
            this.props.refreshUserInfo();
            this.props.openPortal('提示', '注册并登录成功', 'green');
          }
          else {
            this.props.openPortal(`错误 (${result.errorCode})`, `${result.errorMessage}`, 'red');
          }
          element.disabled = false;
        })
        .catch(err => {
          this.props.openPortal('错误', '注册失败', 'red');
          element.disabled = false;
          console.log(err);
        })
    }
  }

  render() {
    return (
      <>
        <Modal size='tiny' open={this.props.modalOpen} closeIcon onClose={this.props.closeModal}>
          <Modal.Header>注册</Modal.Header>
          <Modal.Content>
            <Form id='registerForm'>
              <Form.Field required>
                <Label>用户名</Label>
                <Input name='userName' required></Input>
              </Form.Field>
              <Form.Field required>
                <Label>邮箱</Label>
                <Input name='email' required type='email'></Input>
              </Form.Field>
              <Form.Field required>
                <Label>姓名</Label>
                <Input name='name' required></Input>
              </Form.Field>
              <Form.Field required>
                <Label>密码</Label>
                <Input id='password' name='password' required type='password' placeholder='至少 6 位，包含大小写字母及数字'></Input>
              </Form.Field>
              <Form.Field required>
                <Label>再次输入密码</Label>
                <Input id='confirmPassword' name='confirmPassword' error required type='password'></Input>
              </Form.Field>
              
              <Button onClick={this.register} primary>注册</Button>
            </Form>
          </Modal.Content>
        </Modal>
      </>
    );
  }
}