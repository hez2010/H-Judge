import { Get, Post } from '../../utilities/requestHelper';
import { setTitle } from '../../utilities/titleHelper';

export default {
    props: ['user', 'showSnack'],
    data: () => ({
        problem: {},
        param: {
            pid: 0,
            cid: 0,
            gid: 0
        },
        active: 0,
        loading: true,
        language: { name: '', information: '' },
        languageRules: [
            v => !!v.name || '请选择语言'
        ],
        content: '',
        contentRules: [
            v => !!v || '请输入提交内容'
        ],
        submitting: false,
        valid: false,
        timer: null
    }),
    mounted: function () {
        if (this.$route.params.pid) {
            this.param.pid = parseInt(this.$route.params.pid);
        }
        if (this.$route.params.cid) {
            this.param.cid = parseInt(this.$route.params.cid);
        }
        if (this.$route.params.gid) {
            this.param.gid = parseInt(this.$route.params.gid);
        }
        setTitle('题目详情');
        Get('/Problem/GetProblemDetails', this.param)
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.problem = data;
                    this.$nextTick(() => this.loadMath());
                }
                else this.showSnack(data.errorMessage, 'error', 3000);
                this.loading = false;
            })
            .catch(() => {
                this.showSnack('加载失败', 'error', 3000);
                this.loading = false;
            });
    },
    methods: {
        loadMath: function () {
            this.timer = setInterval(() => {
                if (document.getElementById('details_content')) {
                    clearInterval(this.timer);
                    window.MathJax.Hub.Queue(['Typeset', window.MathJax.Hub]);
                    this.timer = null;
                }
            }, 1000);
        },
        submit: function () {
            if (this.$refs.form.validate()) {
                this.submitting = true;
                let param = this.param;
                param['content'] = this.content;
                if (this.problem.rawType === 1)
                    param['language'] = this.language.name;
                Post('/Problem/Submit', param)
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            this.showSnack('提交成功', 'success', 3000);
                            if (data.redirect)
                                this.$router.push('/Result/' + data.id);
                            else {
                                this.submitting = false;
                            }
                        }
                        else {
                            this.showSnack(data.errorMessage, 'error', 3000);
                            this.submitting = false;
                        }
                    })
                    .catch(() => {
                        this.showSnack('提交失败', 'error', 3000);
                        this.submitting = false;
                    });
            }
        },
        showUser: function (userId) {
            this.$router.push('/Account/' + userId);
        }
    },
    watch: {
        active: function () {
            if (this.active === 3) {
                this.$router.push('/Status/' + (this.param.gid ? this.param.gid.toString() + '/' : '') + (this.param.cid ? this.param.cid.toString() + '/' : '') + this.param.pid.toString() + '/1');
            }
        }
    }
};