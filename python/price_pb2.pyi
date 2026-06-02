from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from typing import ClassVar as _ClassVar, Optional as _Optional

DESCRIPTOR: _descriptor.FileDescriptor

class PriceRequest(_message.Message):
    __slots__ = ("make", "model", "color", "year")
    MAKE_FIELD_NUMBER: _ClassVar[int]
    MODEL_FIELD_NUMBER: _ClassVar[int]
    COLOR_FIELD_NUMBER: _ClassVar[int]
    YEAR_FIELD_NUMBER: _ClassVar[int]
    make: str
    model: str
    color: str
    year: int
    def __init__(self, make: _Optional[str] = ..., model: _Optional[str] = ..., color: _Optional[str] = ..., year: _Optional[int] = ...) -> None: ...

class PriceReply(_message.Message):
    __slots__ = ("currencyCode", "price")
    CURRENCYCODE_FIELD_NUMBER: _ClassVar[int]
    PRICE_FIELD_NUMBER: _ClassVar[int]
    currencyCode: str
    price: int
    def __init__(self, currencyCode: _Optional[str] = ..., price: _Optional[int] = ...) -> None: ...
