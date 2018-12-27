import { Get, Post } from '../../utilities/requestHelper';
import { setTitle } from '../../utilities/titleHelper';
import { ensureLoading } from '../../utilities/scriptHelper';
import { initializeObjects } from '../../utilities/initHelper';
const chatboard = () => import(/* webpackChunkName: 'chatboard' */'../chatboard/chatboard.vue');

export default {
    props: ['user', 'showSnack', 'isDarkTheme'],
    data: () => ({
        active: 0,
        loading: true,
        language: { name: '', information: '', syntaxHighlight: 'plain_text' },
        languageRules: [
            v => !!v.name || '请选择语言'
        ],
        submitting: false,
        valid: false
    }),
    components: {
        chatboard: chatboard
    },
    mounted: function () {
        setTitle('题目详情');

        initializeObjects({
            problem: {},
            param: {
                pid: 0,
                cid: 0,
                gid: 0
            },
            editor: null
        }, this);

        if (this.$route.params.pid) {
            this.param.pid = parseInt(this.$route.params.pid);
        }
        if (this.$route.params.cid) {
            this.param.cid = parseInt(this.$route.params.cid);
        }
        if (this.$route.params.gid) {
            this.param.gid = parseInt(this.$route.params.gid);
        }
        Get('/Problem/GetProblemDetails', this.param)
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.problem = data;
                    this.$nextTick(() => {
                        ensureLoading('mathjax', 'https://cdn.hjudge.com/hjudge/lib/MathJax-2.7.5/MathJax.js?config=TeX-MML-AM_CHTML', this.loadMath);
                        ensureLoading('ace', 'https://cdn.hjudge.com/hjudge/lib/ace/ace.js', this.loadEditor);
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
        loadTheme: function (theme) {
            this.editor.setTheme('ace/theme/' + theme);
        },
        loadEditor: function () {
            this.$nextTick(() => {
                if (document.getElementById('submit_content')) {
                    this.editor = window.ace.edit('submit_content');
                    if (this.isDarkTheme) ensureLoading('ace_theme_twilight', 'https://cdn.hjudge.com/hjudge/lib/ace/theme-twilight.js', () => this.loadTheme('twilight'));
                    else ensureLoading('ace_theme_github', 'https://cdn.hjudge.com/hjudge/lib/ace/theme-github.js', () => this.loadTheme('github'));
                }
            });
        },
        loadMath: function () {
            this.$nextTick(() => {
                if (document.getElementById('details_content')) {
                    window.MathJax.Hub.Queue(['Typeset', window.MathJax.Hub]);
                }
            });
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
        loadSyntaxHighlight: function () {
            let name = this.language.syntaxHighlight ? this.language.syntaxHighlight : 'plain_text';
            let langMode = window.ace.require('ace/mode/' + name).Mode;
            this.editor.session.setMode(new langMode());
        },
        deleteProblem: function (id) {
            if (confirm('确定要删除此题目吗？')) {
                Post('/Admin/DeleteProblem', { id: id })
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            this.showSnack('删除成功', 'success', 3000);
                            this.$router.go(-1);
                        }
                        else this.showSnack(data.errorMessage, 'error', 3000);
                    })
                    .catch(() => this.showSnack('删除失败', 'error', 3000));
            }
        }
    },
    watch: {
        active: function () {
            if (this.active === 1) {
                this.loadMath();
            }
            else if (this.active === 3) {
                this.$router.push('/Status/' + (this.param.gid ? this.param.gid.toString() + '/' : '') + (this.param.cid ? this.param.cid.toString() + '/' : '') + this.param.pid.toString() + '/1');
            }
        },
        language: function () {
            let name = this.language.syntaxHighlight ? this.language.syntaxHighlight : 'plain_text';
            ensureLoading('ace_mode_' + name, 'https://cdn.hjudge.com/hjudge/lib/ace/mode-' + name + '.js', this.loadSyntaxHighlight);
        },
        isDarkTheme: function () {
            if (this.editor)
                this.editor.setTheme('ace/theme/' + (this.isDarkTheme ? 'twilight' : 'github'));
        }
    }
};