import { Get, Post } from '../../../utilities/requestHelper';
import { setTitle } from '../../../utilities/titleHelper';

export default {
    data: () => ({
        config: {},
        loading: true,
        valid: false,
        requireRules: [
            v => !!v || '请填写该项内容'
        ],
        submitting: false
    }),
    mounted: function () {
        setTitle('系统设置');
        Get('/Admin/GetSystemConfig')
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.config = data;
                }
                else alert(data.errorMessage);
                this.loading = false;
            })
            .catch(() => {
                alert('加载失败');
                this.loading = false;
            });
    },
    methods: {
        submit: function () {
            if (this.$refs.form.validate()) {
                this.submitting = true;
                Post('/Admin/UpdateSystemConfig', this.config)
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) alert('保存成功');
                        else alert(data.errorMessage);
                        this.submitting = false;
                    })
                    .catch(() => {
                        alert('保存失败');
                        this.submitting = false;
                    });
            }
        },
        add: function () {
            this.config.languages = this.config.languages.concat([{
                name: '',
                extensions: '',
                compilerExec: '',
                compilerArgs: '',
                compilerProblemMatcher: '',
                compilerDisplayFormat: '',
                compilerReadStdOutput: true,
                compilerReadStdError: true,
                staticCheckExec: '',
                staticCheckArgs: '',
                staticCheckProblemMatcher: '',
                staticCheckDisplayFormat: '',
                staticCheckReadStdOutput: true,
                staticCheckReadStdError: true,
                runExec: '',
                runArgs: ''
            }]);
        },
        remove: function (index) {
            this.config.languages.splice(index, 1);
        }
    }
};