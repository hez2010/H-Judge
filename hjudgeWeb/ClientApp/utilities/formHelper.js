export function SerializeForm(form) {
    if (!form || form.nodeName !== 'FORM') {
        return;
    }
    var i, j, q = {};
    for (i = form.elements.length - 1; i >= 0; i = i - 1) {
        if (form.elements[i].name === '') {
            continue;
        }
        switch (form.elements[i].nodeName) {
        case 'INPUT':
            switch (form.elements[i].type) {
            case 'text':
            case 'hidden':
            case 'password':
            case 'button':
            case 'reset':
            case 'submit':
            case 'email':
            case 'number':
                q[form.elements[i].name] = form.elements[i].value.toString();
                break;
            case 'checkbox':
            case 'radio':
                if (form.elements[i].checked) {
                    q[form.elements[i].name] = form.elements[i].value.toString();
                }
                break;
            }
            break;
        case 'file':
            break;
        case 'TEXTAREA':
            q[form.elements[i].name] = form.elements[i].value.toString();
            break;
        case 'SELECT':
            switch (form.elements[i].type) {
            case 'select-one':
                q[form.elements[i].name] = form.elements[i].value.toString();
                break;
            case 'select-multiple':
                q[form.elements[i].name] = [];
                for (j = form.elements[i].options.length - 1; j >= 0; j = j - 1) {
                    if (form.elements[i].options[j].selected) {
                        q[form.elements[i].name].push(form.elements[i].value.toString());
                    }
                }
                break;
            }
            break;
        case 'BUTTON':
            switch (form.elements[i].type) {
            case 'reset':
            case 'submit':
            case 'button':
                q[form.elements[i].name] = form.elements[i].value.toString();
                break;
            }
            break;
        }
    }
    return q;
}