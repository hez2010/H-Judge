import { Get, Post } from '../../utilities/requestHelper';
import { setTitle } from '../../utilities/titleHelper';

export default {
    props: ['user'],
    data: () => ({
        problem: {},
        param: {
            pid: 0,
            cid: 0,
            gid: 0
        },
        loading: true,
        language: '',
        languageRules: [
            v => !!v || '请选择语言'
        ],
        content: '',
        contentRules: [
            v => !!v || '请输入提交内容'
        ],
        submitting: false,
        valid: false
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
                if (data.isSucceeded) this.problem = data;
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
                let param = this.param;
                param['content'] = this.content;
                if (this.problem.rawType === 1)
                    param['language'] = this.language;
                Post('/Problem/Submit', param)
                    .then(res => res.json())
                    .then(data => {
                        if (data.isSucceeded) {
                            if (data.redirect)
                                this.$router.push('/Result/' + data.id);
                            else {
                                alert('提交成功');
                                this.submitting = false;
                            }
                        }
                        else {
                            alert(data.errorMessage);
                            this.submitting = false;
                        }
                    })
                    .catch(() => {
                        alert('提交失败');
                        this.submitting = false;
                    });
            }
        }
    }
};