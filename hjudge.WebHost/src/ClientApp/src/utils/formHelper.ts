export function SerializeForm(form: HTMLFormElement) {
  if (!form || form.nodeName !== "FORM") {
    return;
  }
  let q: any = {};

  for (let i = form.elements.length - 1; i >= 0; i = i - 1) {
    switch (form.elements[i].nodeName.toLowerCase()) {
      case 'input': {
        let element = form.elements[i] as HTMLInputElement;
        switch (element.type.toLowerCase()) {
          case 'text':
          case 'hidden':
          case 'password':
          case 'button':
          case 'reset':
          case 'submit':
          case 'email':
          case 'number':
            if (!element.name) break;
            q[element.name] = element.value;
            break;
          case 'checkbox':
          case 'radio':
            if (!element.name) break;
            q[element.name] = element.checked;
            break;
        }
        break;
      }
      case 'file':
        break;
      case 'textarea': {
        let element = form.elements[i] as HTMLTextAreaElement;
        if (!element.name) break;
        q[element.name] = element.value;
        break;
      }
      case 'select': {
        let element = form.elements[i] as HTMLSelectElement;
        if (!element.name) break;
        switch (element.type) {
          case 'select-one':
            q[element.name] = element.value;
            break;
          case 'select-multiple':
            q[element.name] = [];
            for (let j = element.options.length - 1; j >= 0; j = j - 1) {
              if (element.options[j].selected) {
                q[element.name].push(element.value);
              }
            }
            break;
        }
        break;
      }
      case 'button': {
        let element = form.elements[i] as HTMLButtonElement;
        if (!element.name) break;
        switch (element.type) {
          case 'reset':
          case 'submit':
          case 'button':
            q[element.name] = element.value;
            break;
        }
        break;
      }
    }
  }
  return q;
}