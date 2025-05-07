// THIS FUNCTION USE FOR INDIAN CURRENCY FORMAT
function onlyNum(event) {
  var ASCIICODE = event.which || event.keyCode;
  if ((ASCIICODE >= 48 && ASCIICODE <= 57) || ASCIICODE === 46) {
    return true;
  }
  return false;
}

// USED TO ACCEPT NUMBER AND DECIMAL AND CONVERT IN INDIAN COMMA FORMAT
document.querySelectorAll('.NumberCommaSaperator').forEach(function (input) {
  input.addEventListener('keyup', function (event) {
    var number = this.value.trim().replace(/,/g, '');
    var index = number.indexOf('.');
    var intPart = index === -1 ? number : number.substring(0, index);
    var decimalPart = index === -1 ? '' : number.substring(index + 1);
    var formattedNumber = (+intPart).toLocaleString('en-IN', { useGrouping: true });

    if (event.key !== '.') {
      this.value = decimalPart === '' ? formattedNumber : formattedNumber + '.' + decimalPart.substring(0, 2);
    }
  });
});

// ALLOW ONLY CHARACTERS AND SPACE
document.querySelectorAll('.onlycharacter').forEach(function (input) {
  input.addEventListener('keypress', function (event) {
    if (this.value.length === 0 && event.which === 32) event.preventDefault();
    var regex = /^[a-zA-Z]+$/;
    if (!regex.test(event.key)) {
      event.preventDefault();
    }
  });
});

// ALLOW ONLY NUMBERS AND TWO DECIMAL POINTS
document.querySelectorAll('.numbers').forEach(function (input) {
  input.addEventListener('input', function (event) {
    var charCode = event.which || event.keyCode;
    if (charCode !== 46 && (charCode < 48 || charCode > 57)) {
      event.preventDefault();
    }
    if (charCode === 46) {
      if (this.value.indexOf('.') !== -1) {
        event.preventDefault();
      }
    }
    if (this.value.indexOf('.') > 0) {
      var parts = this.value.split('.');
      if (parts[1].length > 2) {
        this.value = parts[0] + '.' + parts[1].substring(0, 2);
      }
    }
  });
});

// ALLOW ONLY NUMERIC INPUT
document.querySelectorAll('.numberOnly').forEach(function (input) {
  input.addEventListener('keypress', function (event) {
    var regex = /^[0-9]+$/;
    if (!regex.test(event.key)) {
      event.preventDefault();
    }
  });
});

// ALLOW SPECIAL CHARACTERS
document.querySelectorAll('.specialcharacter').forEach(function (input) {
  input.addEventListener('keypress', function (event) {
    if (this.value.length === 0 && event.which === 32) event.preventDefault();
    var regex = /^[a-zA-Z. ]+$/;
    if (!regex.test(event.key)) {
      event.preventDefault();
    }
  });
});

// ALLOW ALPHANUMERIC CHARACTERS
document.querySelectorAll('.numbercharacter').forEach(function (input) {
  input.addEventListener('keypress', function (event) {
    if (this.value.length === 0 && event.which === 32) event.preventDefault();
    var regex = /^[a-zA-Z0-9 ]+$/;
    if (!regex.test(event.key)) {
      event.preventDefault();
    }
  });
});

// ALLOW NARRATION SPECIAL CHARACTERS
document.querySelectorAll('.narration').forEach(function (input) {
  input.addEventListener('keypress', function (event) {
    if (this.value.length === 0 && event.which === 32) event.preventDefault();
    var regex = /^[#/?,()%$!a-zA-Z0-9. ]+$/;
    if (!regex.test(event.key)) {
      event.preventDefault();
    }
  });
});

// FORMAT VALUE TO TWO DECIMAL PLACES
document.querySelectorAll('.makeTwoDecimal').forEach(function (input) {
  input.addEventListener('change', function () {
    this.value = this.value ? parseFloat(this.value).toFixed(2) : '';
  });
});

// ALLOW ONLY CHARACTERS AND DASH
document.querySelectorAll('.onlyCharacterAndDash').forEach(function (input) {
  input.addEventListener('keypress', function (event) {
    if (this.value.length === 0 && (event.which === 32 || event.which === 47 || event.which === 95)) {
      event.preventDefault();
    }

    var regex = /^[a-zA-Z -_]+$/;
    if (!regex.test(event.key)) {
      event.preventDefault();
    }

    var lastChar = this.value.slice(-1);
    if (
      (event.which === 32 && lastChar === ' ') ||
      (event.which === 45 && lastChar === '-') ||
      (event.which === 95 && lastChar === '_')
    ) {
      event.preventDefault();
    }

    if (
      (event.which === 32 || event.which === 45 || event.which === 95) &&
      (lastChar === ' ' || lastChar === '-' || lastChar === '_')
    ) {
      event.preventDefault();
    }

    if (this.value.includes(' ') && event.which === 32) {
      event.preventDefault();
    }
    if (this.value.includes('-') && event.which === 45) {
      event.preventDefault();
    }
    if (this.value.includes('_') && event.which === 95) {
      event.preventDefault();
    }
  });
});
