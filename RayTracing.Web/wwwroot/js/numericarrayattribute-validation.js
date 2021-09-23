$(function () {

    jQuery.validator.unobtrusive.adapters.add("numericarray",
        function (options) {
            options.rules["numericarray"] = options.params;
            options.messages["numericarray"] = options.message
        });

}(jQuery));

function isString(x) {
    return Object.prototype.toString.call(x) === "[object String]"
}

(function ($) {
    jQuery.validator.addMethod("numericarray",
        function (value, element, parameters) {
            return isString(value) && !value.split(' ').some(isNaN)
        }
    );
})(jQuery);