$(document).ready(function () {
    $('#button').click(function () {
        event.stopPropagation();
        console.log('Button clicked using jQuery!');
    });
});