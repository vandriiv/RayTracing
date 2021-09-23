$(function () {

    jQuery.validator.unobtrusive.adapters.add("numericmatrix",
        function (options) {
            options.rules["numericmatrix"] = options.params;
            options.messages["numericmatrix"] = options.message
        });

}(jQuery));

function isString(x) {
    return Object.prototype.toString.call(x) === "[object String]"
}

function isValidMatrix(x) {
    const rows = x.split(/\r?\n/);

    return !rows.some(value => value.split(' ').some(isNaN));
}

(function ($) {
    jQuery.validator.addMethod("numericmatrix",
        function (value, element, parameters) {
            return isString(value) && isValidMatrix(value);
        }
    );
})(jQuery);