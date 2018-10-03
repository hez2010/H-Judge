import { Get } from '../../../utilities/requestHelper';

export default {
    data: () => ({
        problem: {},
        timer: null
    }),
    mounted: function () {
        Get('/Admin/GetProblemConfig');
        this.$nextTick(() => {
            this.timer = setInterval(function () {
                if (window.CKEDITOR) {
                    window.CKEDITOR.replace('editor');
                    clearInterval(this.timer);
                }
            }, 200);
        });
    }
};