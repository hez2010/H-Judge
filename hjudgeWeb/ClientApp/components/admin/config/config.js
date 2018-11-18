import { Get, Post } from '../../../utilities/requestHelper';
import { setTitle } from '../../../utilities/titleHelper';

export default {
    props: ['showSnack'],
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
        setTitle('设置');
        Get('/Admin/GetSystemConfig')
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.config = data;
                }
                else
                    this.showSnack(data.errorMessage, 'error', 3000);
                this.loading = false;
            })
            .catch(() => {
                this.showSnack('加载失败', 'error', 3000);
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
                        if (data.isSucceeded)
                            this.showSnack('保存成功', 'success', 3000);
                        else
                            this.showSnack(data.errorMessage, 'error', 3000);
                        this.submitting = false;
                    })
                    .catch(() => {
                        this.showSnack('保存失败', 'error', 3000);
                        this.submitting = false;
                    });
            }
        },
        add: function () {
            this.config.languages = this.config.languages.concat([{
                name: '',
                extensions: '',
                information: '',
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