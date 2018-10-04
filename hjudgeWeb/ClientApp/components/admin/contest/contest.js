import { Get, Post } from '../../../utilities/requestHelper';
import { setTitle } from '../../../utilities/titleHelper';

export default {
    data: () => ({
        contest: {},
        valid: false,
        loading: true,
        requireRules: [
            v => !!v || '此项不能为空'
        ],
        timer: null,
        submitting: false
    }),
    mounted: function () {
        setTitle('比赛编辑');
        Get('/Admin/GetContestConfig', { cid: this.$route.params.cid })
            .then(res => res.json())
            .then(data => {
                if (data.isSucceeded) {
                    this.contest = data;
                    this.$nextTick(() => this.timer = setInterval(() => this.loadEditor(), 200));
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
        loadEditor: function () {
            if (window.CKEDITOR) {
                window.CKEDITOR.replace('editor');
                clearInterval(this.timer);
            }
        },
        isValid: function () {
            return this.valid && this.contest.startTime && this.contest.endTime;
        },
        save: function () {
            if (!this.$refs.basic.validate()) return;
            if (!this.isValid()) return;
            this.contest.description = window.CKEDITOR.instances['editor'].getData();
            this.submitting = true;
            Post('/Admin/UpdateContestConfig', this.contest)
                .then(res => res.json())
                .then(data => {
                    if (data.isSucceeded) {
                        if (this.contest.id === 0) {
                            this.$router.push('/Admin/Contest/' + data.id.toString());
                            this.contest.id = data.id;
                        }
                        alert('保存成功');
                    }
                    else alert(data.errorMessage);
                    this.submitting = false;
                })
                .catch(() => {
                    alert('保存失败');
                    this.submitting = false;
                });
        }
    }
};