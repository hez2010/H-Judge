import { Get, Post } from '../../utilities/requestHelper';
import { setTitle } from '../../utilities/titleHelper';

export default {
    props: ['user', 'showSnack', 'isDarkTheme'],
    data: () => ({
        problem: {},
        param: {
            pid: 0,
            cid: 0,
            gid: 0
        },
        active: 0,
        loading: true,
        language: { name: '', information: '', syntaxHighlight: 'plain_text' },
        languageRules: [
            v => !!v.name || '请选择语言'
        ],
        submitting: false,
        valid: false,
        timer1: null,
        timer2: null,
        editor: null
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
                    this.$nextTick(() => {
                        this.loadMath();
                        this.loadEditor();
                    });
                }
                else this.showSnack(data.errorMessage, 'error', 3000);
                this.loading = false;
            })
            .catch(() => {
                this.showSnack('加载失败', 'error', 3000);
                this.loading = false;
            });
    },
    destroyed: function () {
        if (this.editor) {
            this.editor.destroy();
            this.editor.container.remove();
            this.editor = null;
        }
    },
    methods: {
        loadEditor: function () {
            this.timer1 = setInterval(() => {
                if (document.getElementById('submit_content')) {
                    clearInterval(this.timer1);
                    this.editor = window.ace.edit('submit_content');
                    this.editor.setTheme('ace/theme/' + (this.isDarkTheme ? 'twilight' : 'github'));
                    this.timer1 = null;
                }
            }, 1000);
        },
        loadMath: function () {
            this.timer2 = setInterval(() => {
                if (document.getElementById('details_content')) {
                    clearInterval(this.timer2);
                    window.MathJax.Hub.Queue(['Typeset', window.MathJax.Hub]);
                    this.timer2 = null;
                }
            }, 1000);
        },
        submit: function () {
            if (this.$refs.form.validate() && this.editor) {
                let content = this.editor.getValue();
                if (content) {
                    this.submitting = true;
                    let param = this.param;
                    param['content'] = content;
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
                else this.showSnack('请输入提交内容', 'error', 3000);
            }
        },
        showUser: function (userId) {
            this.$router.push('/Account/' + userId);
        },
        loadSyntaxHighlight: function () {
            let name = this.language.syntaxHighlight ? this.language.syntaxHighlight : 'plain_text';
            let langMode = window.ace.require('ace/mode/' + name).Mode;
            this.editor.session.setMode(new langMode());
        }
    },
    watch: {
        active: function () {
            if (this.active === 3) {
                this.$router.push('/Status/' + (this.param.gid ? this.param.gid.toString() + '/' : '') + (this.param.cid ? this.param.cid.toString() + '/' : '') + this.param.pid.toString() + '/1');
            }
        },
        language: function () {
            let name = this.language.syntaxHighlight ? this.language.syntaxHighlight : 'plain_text';
            if (!document.getElementById('ace_mode_' + name)) {
                let script = document.createElement('script');
                script.id = 'ace_mode_' + name;
                script.type = 'text/javascript';
                script.src = 'https://cdn.hjudge.com/hjudge/lib/ace/mode-' + name + '.js';
                document.body.appendChild(script);
                script.onload = this.loadSyntaxHighlight;
            }
            else this.loadSyntaxHighlight();
        },
        isDarkTheme: function () {
            if (this.editor)
                this.editor.setTheme('ace/theme/' + (this.isDarkTheme ? 'twilight' : 'github'));
        }
    }
};