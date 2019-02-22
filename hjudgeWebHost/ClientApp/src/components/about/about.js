"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var React = require("react");
var semantic_ui_react_1 = require("semantic-ui-react");
var titleHelper_1 = require("../../utils/titleHelper");
var About = /** @class */ (function (_super) {
    __extends(About, _super);
    function About() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    About.prototype.componentDidMount = function () {
        titleHelper_1.setTitle('关于');
    };
    About.prototype.render = function () {
        return (React.createElement("div", null,
            React.createElement(semantic_ui_react_1.Header, { as: 'h1' }, "\u5173\u4E8E H::Judge")));
    };
    return About;
}(React.Component));
exports.default = About;
//# sourceMappingURL=about.js.map